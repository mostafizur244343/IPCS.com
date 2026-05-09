using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Repo.Data;
using IPCS_Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPCS_Service.Implementation
{
    public class TransferService : ITransferService
    {
        private readonly IPCSDBContext _context;
        private readonly IProductService _productService;

        public TransferService(IPCSDBContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        private async Task<string> GenerateCodeAsync(string prefix, DbSet<TransferMaster> dbSetMaster = null, DbSet<TransferRequisition> dbSetReq = null)
        {
            int nextId = 1;
            if (dbSetMaster != null)
            {
                var last = await dbSetMaster.IgnoreQueryFilters().AsNoTracking().OrderByDescending(x => x.TransferId).FirstOrDefaultAsync();
                if (last != null) nextId = last.TransferId + 1;
            }
            else if (dbSetReq != null)
            {
                var last = await dbSetReq.IgnoreQueryFilters().AsNoTracking().OrderByDescending(x => x.RequisitionId).FirstOrDefaultAsync();
                if (last != null) nextId = last.RequisitionId + 1;
            }
            return $"{prefix}-{nextId:D5}";
        }

        // Requisitions
        public async Task<IEnumerable<TransferRequisition>> GetAllRequisitionsAsync()
        {
            return await _context.Set<TransferRequisition>()
                .Include(r => r.FromBranch)
                .Include(r => r.ToBranch)
                .Where(r => !r.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TransferRequisition?> GetRequisitionByIdAsync(int id)
        {
            return await _context.Set<TransferRequisition>()
                .AsNoTracking()
                .Include(r => r.FromBranch)
                .Include(r => r.ToBranch)
                .Include(r => r.RequisitionDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(r => r.RequisitionId == id && !r.IsDeleted);
        }

        public async Task<bool> CreateRequisitionAsync(TransferRequisition requisition)
        {
            requisition.RequisitionCode = await GenerateCodeAsync("REQ", null, _context.Set<TransferRequisition>());
            
            // Calculate Base Unit Quantity using ProductService
            foreach(var detail in requisition.RequisitionDetails)
            {
                detail.RequestQtyInPcs = await _productService.ConvertToBaseUnit(detail.ProductId, detail.UOMId, detail.RequestQty);
            }

            await _context.AddAsync(requisition);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateRequisitionStatusAsync(int id, string status)
        {
            var req = await _context.Set<TransferRequisition>().FindAsync(id);
            if (req == null) return false;
            req.Status = status;
            return await _context.SaveChangesAsync() > 0;
        }

        // Transfers
        public async Task<IEnumerable<TransferMaster>> GetAllTransfersAsync()
        {
            return await _context.Set<TransferMaster>()
                .Include(t => t.FromBranch)
                .Include(t => t.ToBranch)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.TransferDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TransferMaster?> GetTransferByIdAsync(int id)
        {
            return await _context.Set<TransferMaster>()
                .AsNoTracking()
                .Include(t => t.FromBranch)
                .Include(t => t.ToBranch)
                .Include(t => t.TransferDetails)
                    .ThenInclude(d => d.Product)
                .Include(t => t.TransferDetails)
                    .ThenInclude(d => d.Lot)
                .FirstOrDefaultAsync(t => t.TransferId == id && !t.IsDeleted);
        }

        public async Task<bool> InitiateTransferAsync(TransferMaster transferMaster)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                transferMaster.TransferCode = await GenerateCodeAsync("TRN", _context.Set<TransferMaster>(), null);
                transferMaster.Status = "In-Transit"; // Changed from Pending to In-Transit as per logic requirements

                await _context.AddAsync(transferMaster);

                // Deduct stock from Sender Branch
                foreach (var detail in transferMaster.TransferDetails)
                {
                    // Fetch CostPrice from Lot
                    var lot = await _context.Set<LotInfo>().AsNoTracking().FirstOrDefaultAsync(l => l.LotId == detail.LotId);
                    detail.CostPrice = lot?.PurchasePrice ?? 0;

                    // Dynamic Conversion to PCS using ProductService
                    detail.TransferQtyInPcs = await _productService.ConvertToBaseUnit(detail.ProductId, detail.UOMId, detail.TransferQty);
                    
                    // Set LineTotal
                    detail.LineTotal = detail.TransferQty * detail.CostPrice;

                    // Only deduct stock if status is In-Transit (Shipped)
                    if (transferMaster.Status == "In-Transit")
                    {
                        await UpdateStockAsync(transferMaster.FromBranchId, detail.ProductId, detail.LotId, detail.TransferQtyInPcs, false, "Transfer Out", transferMaster.TransferCode, transferMaster.CreatedBy);
                    }
                }

                await _context.SaveChangesAsync();

                if (transferMaster.RequisitionId.HasValue)
                {
                    var req = await _context.Set<TransferRequisition>()
                        .Include(r => r.RequisitionDetails)
                        .FirstOrDefaultAsync(r => r.RequisitionId == transferMaster.RequisitionId.Value);
                        
                    if (req != null)
                    {
                        bool isPartial = false;
                        foreach (var reqDetail in req.RequisitionDetails)
                        {
                            var transferredTotalForProduct = transferMaster.TransferDetails
                                .Where(td => td.ProductId == reqDetail.ProductId)
                                .Sum(td => td.TransferQtyInPcs);

                            // Update ApprovedQtyInPcs with the newly transferred quantity
                            reqDetail.ApprovedQtyInPcs += transferredTotalForProduct;

                            if (reqDetail.ApprovedQtyInPcs < reqDetail.RequestQtyInPcs)
                            {
                                isPartial = true;
                            }
                        }

                        // Determine if full or partial based on overall progress
                        req.Status = isPartial ? "Partially Transferred" : "Transferred";
                        _context.Update(req);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error Initiating Transfer: " + ex.Message, ex);
            }
        }

        public async Task<bool> ConfirmGoodsReceivedAsync(TransferReceiveDTO receiveDto, string receivedBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.Set<TransferMaster>()
                    .Include(t => t.TransferDetails)
                    .FirstOrDefaultAsync(t => t.TransferId == receiveDto.TransferId);

                if (transfer == null || transfer.Status != "In-Transit") return false;

                transfer.Status = "Received";
                transfer.ReceivedDate = DateTime.Now;
                transfer.ReceivedBy = receivedBy;

                foreach (var detail in transfer.TransferDetails)
                {
                    var receivedInfo = receiveDto.Details.FirstOrDefault(d => d.TransferDetailId == detail.TransferDetailId);
                    
                    if (receivedInfo != null)
                    {
                        if (receivedInfo.ReceivedQty > detail.TransferQty)
                            throw new Exception($"Received quantity ({receivedInfo.ReceivedQty}) cannot be greater than transferred quantity ({detail.TransferQty}) for Product ID: {detail.ProductId}");

                        detail.ReceivedQty = receivedInfo.ReceivedQty;
                        
                        // Dynamic Conversion to PCS using ProductService
                        detail.ReceivedQtyInPcs = await _productService.ConvertToBaseUnit(detail.ProductId, detail.UOMId, detail.ReceivedQty);
                        
                        // Calculate Damage
                        detail.DamageQty = detail.TransferQty - detail.ReceivedQty;
                        detail.DamageQtyInPcs = detail.TransferQtyInPcs - detail.ReceivedQtyInPcs;
                    }
                    else
                    {
                        // Default to full transfer if not explicitly updated
                        detail.ReceivedQty = detail.TransferQty;
                        detail.ReceivedQtyInPcs = detail.TransferQtyInPcs;
                        detail.DamageQty = 0;
                        detail.DamageQtyInPcs = 0;
                    }
                    
                    decimal actualReceivedQtyPcs = detail.ReceivedQtyInPcs;
                    decimal damagedQtyPcs = detail.DamageQtyInPcs;

                    // 2. Update Receiving Branch Stock (QUANTITY RECEIVED)
                    await UpdateStockAsync(transfer.ToBranchId, detail.ProductId, detail.LotId, actualReceivedQtyPcs, true, "Transfer In", transfer.TransferCode, receivedBy);

                    // 3. Update Damaged Stock at Source Branch (NON-SELLABLE)
                    if (damagedQtyPcs > 0)
                    {
                        await UpdateDamagedStockAsync(transfer.FromBranchId, detail.ProductId, detail.LotId, damagedQtyPcs, transfer.TransferCode, receivedBy);

                        // MAJOR UPDATE: Update Global Product Stock (Deduct sellable loss)
                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.CurrentStock -= damagedQtyPcs;
                            _context.Products.Update(product);
                        }
                    }
                }

                _context.Update(transfer);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error Receiving Transfer: " + ex.Message, ex);
            }
        }

        public async Task<bool> CancelTransferAsync(int transferId)
        {
            var transfer = await _context.Set<TransferMaster>().FindAsync(transferId);
            if (transfer == null || (transfer.Status != "In-Transit" && transfer.Status != "Pending")) return false;

            transfer.Status = "Canceled";
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ConfirmCancelReturnAsync(int transferId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.Set<TransferMaster>().Include(t => t.TransferDetails).FirstOrDefaultAsync(t => t.TransferId == transferId);
                if (transfer == null || transfer.Status != "Canceled") return false;

                transfer.Status = "CancelConfirmed";
                _context.Update(transfer);

                // Add stock back to Sender Branch
                foreach (var detail in transfer.TransferDetails)
                {
                    await UpdateStockAsync(transfer.FromBranchId, detail.ProductId, detail.LotId, detail.TransferQtyInPcs, true, "Transfer Return In", transfer.TransferCode, transfer.CreatedBy);
                }

                // Update Requisition if linked
                if (transfer.RequisitionId.HasValue)
                {
                    var req = await _context.Set<TransferRequisition>()
                        .Include(r => r.RequisitionDetails)
                        .FirstOrDefaultAsync(r => r.RequisitionId == transfer.RequisitionId.Value);

                    if (req != null)
                    {
                        foreach (var detail in transfer.TransferDetails)
                        {
                            var reqDetail = req.RequisitionDetails.FirstOrDefault(rd => rd.ProductId == detail.ProductId);
                            if (reqDetail != null)
                            {
                                reqDetail.ApprovedQtyInPcs -= detail.TransferQtyInPcs;
                            }
                        }

                        // Determine new status
                        bool anyApproved = req.RequisitionDetails.Any(rd => rd.ApprovedQtyInPcs > 0);
                        if (!anyApproved)
                        {
                            req.Status = "Pending";
                        }
                        else
                        {
                            req.Status = "Partially Transferred";
                        }
                        _context.Update(req);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error Confirming Transfer Cancel Return: " + ex.Message, ex);
            }
        }

        public async Task<bool> EditPendingTransferAsync(TransferMaster updatedTransfer)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingTransfer = await _context.Set<TransferMaster>()
                    .Include(t => t.TransferDetails)
                    .FirstOrDefaultAsync(t => t.TransferId == updatedTransfer.TransferId);

                if (existingTransfer == null || (existingTransfer.Status != "In-Transit" && existingTransfer.Status != "Pending")) return false;

                // 1. Give back the stock from Old Details to FromBranch
                foreach (var detail in existingTransfer.TransferDetails)
                {
                    await UpdateStockAsync(existingTransfer.FromBranchId, detail.ProductId, detail.LotId, detail.TransferQtyInPcs, true, "Transfer Edit Return", existingTransfer.TransferCode, "System");
                }

                // 2. Remove old Details
                _context.Set<TransferDetails>().RemoveRange(existingTransfer.TransferDetails);
                await _context.SaveChangesAsync(); 

                // 3. Update Master
                existingTransfer.ToBranchId = updatedTransfer.ToBranchId;
                existingTransfer.Remarks = updatedTransfer.Remarks;

                // Handle Requisition updates if linked
                TransferRequisition? req = null;
                if (existingTransfer.RequisitionId.HasValue)
                {
                    req = await _context.Set<TransferRequisition>()
                        .Include(r => r.RequisitionDetails)
                        .FirstOrDefaultAsync(r => r.RequisitionId == existingTransfer.RequisitionId.Value);

                    if (req != null)
                    {
                        // 3.1 Subtract old quantities from Requisition tracking
                        foreach (var oldDetail in existingTransfer.TransferDetails)
                        {
                            var reqDetail = req.RequisitionDetails.FirstOrDefault(rd => rd.ProductId == oldDetail.ProductId);
                            if (reqDetail != null)
                            {
                                reqDetail.ApprovedQtyInPcs -= oldDetail.TransferQtyInPcs;
                            }
                        }
                    }
                }

                // 4. Add new details and deduct stock
                foreach (var newDetail in updatedTransfer.TransferDetails)
                {
                    // Fetch CostPrice from Lot
                    var lot = await _context.Set<LotInfo>().AsNoTracking().FirstOrDefaultAsync(l => l.LotId == newDetail.LotId);
                    decimal costPrice = lot?.PurchasePrice ?? 0;

                    // Dynamic Conversion to PCS using ProductService
                    decimal currentTransferQtyInPcs = await _productService.ConvertToBaseUnit(newDetail.ProductId, newDetail.UOMId, newDetail.TransferQty);

                    var nd = new TransferDetails
                    {
                        TransferId = existingTransfer.TransferId,
                        ProductId = newDetail.ProductId,
                        UOMId = newDetail.UOMId,
                        LotId = newDetail.LotId,
                        TransferQty = newDetail.TransferQty,
                        TransferQtyInPcs = currentTransferQtyInPcs,
                        UnitPrice = newDetail.UnitPrice,
                        CostPrice = costPrice,
                        LineTotal = newDetail.TransferQty * costPrice
                    };
                    await _context.Set<TransferDetails>().AddAsync(nd);
                    
                    // Deduct stock for new transfer setup
                    await UpdateStockAsync(existingTransfer.FromBranchId, newDetail.ProductId, newDetail.LotId, currentTransferQtyInPcs, false, "Transfer Out (Edited)", existingTransfer.TransferCode, existingTransfer.CreatedBy);

                    // 4.1 Update Requisition tracking with new quantities
                    if (req != null)
                    {
                        var reqDetail = req.RequisitionDetails.FirstOrDefault(rd => rd.ProductId == newDetail.ProductId);
                        if (reqDetail != null)
                        {
                            reqDetail.ApprovedQtyInPcs += currentTransferQtyInPcs;
                        }
                    }
                }

                // 5. Update Requisition Status if linked
                if (req != null)
                {
                    bool isPartial = false;
                    foreach (var rd in req.RequisitionDetails)
                    {
                        if (rd.ApprovedQtyInPcs < rd.RequestQtyInPcs)
                        {
                            isPartial = true;
                            break;
                        }
                    }
                    req.Status = isPartial ? "Partially Transferred" : "Transferred";
                    _context.Update(req);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error Editing Transfer: " + ex.Message, ex);
            }
        }

        private async Task UpdateStockAsync(int branchId, int productId, int lotId, decimal qty, bool isAdding, string transactionType, string referenceNo, string createdBy)
        {
            // Update BranchLotStock
            var branchLotStock = await _context.BranchLotStocks
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.ProductId == productId && b.LotId == lotId);

            if (branchLotStock == null)
            {
                if (!isAdding) throw new Exception("Insufficient stock to deduct.");
                
                branchLotStock = new BranchLotStock
                {
                    BranchId = branchId,
                    ProductId = productId,
                    LotId = lotId,
                    CurrentStock = qty
                };
                await _context.BranchLotStocks.AddAsync(branchLotStock);
            }
            else
            {
                if (!isAdding && branchLotStock.CurrentStock < qty) 
                    throw new Exception($"Insufficient stock in Branch. Stock is {branchLotStock.CurrentStock}, Requested: {qty}");
                    
                branchLotStock.CurrentStock = isAdding ? branchLotStock.CurrentStock + qty : branchLotStock.CurrentStock - qty;
                _context.BranchLotStocks.Update(branchLotStock);
            }

            // Update StockLedger
            var lastLedger = await _context.StockLedgers
                .AsNoTracking()
                .Where(l => l.BranchId == branchId && l.ProductId == productId && l.LotId == lotId)
                .OrderByDescending(l => l.LedgerId)
                .FirstOrDefaultAsync();

            decimal previousBalance = lastLedger?.CurrentBalance ?? 0;

            var ledger = new StockLedger
            {
                BranchId = branchId,
                ProductId = productId,
                LotId = lotId,
                TransactionType = transactionType,
                ReferenceNo = referenceNo,
                TransactionDate = DateTime.Now,
                PreviousBalance = previousBalance,
                QuantityIn = isAdding ? qty : 0,
                QuantityOut = isAdding ? 0 : qty
            };
            await _context.StockLedgers.AddAsync(ledger);
        }

        private async Task UpdateDamagedStockAsync(int branchId, int productId, int lotId, decimal damagedQty, string referenceNo, string createdBy)
        {
            var branchLotStock = await _context.BranchLotStocks
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.ProductId == productId && b.LotId == lotId);

            if (branchLotStock == null)
            {
                branchLotStock = new BranchLotStock
                {
                    BranchId = branchId,
                    ProductId = productId,
                    LotId = lotId,
                    DamagedStock = damagedQty
                };
                await _context.BranchLotStocks.AddAsync(branchLotStock);
            }
            else
            {
                branchLotStock.DamagedStock += damagedQty;
                _context.BranchLotStocks.Update(branchLotStock);
            }

            // Log in Ledger as Adjustment/Loss (Doesn't affect current sellable balance)
            var ledger = new StockLedger
            {
                BranchId = branchId,
                ProductId = productId,
                LotId = lotId,
                TransactionType = "Transfer Damage",
                ReferenceNo = referenceNo,
                TransactionDate = DateTime.Now,
                PreviousBalance = branchLotStock.CurrentStock,
                QuantityIn = 0,
                QuantityOut = 0,
                Remarks = $"Damaged during transfer. Recorded as non-sellable stock."
            };
            await _context.StockLedgers.AddAsync(ledger);
        }

        public async Task<bool> SoftDeleteTransferAsync(int id)
        {
            var transfer = await _context.Set<TransferMaster>().FindAsync(id);
            if (transfer == null) return false;
            transfer.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
