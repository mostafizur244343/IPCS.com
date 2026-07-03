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
    public class PurchaseService : IPurchaseService
    {
        private readonly IPCSDBContext _context;

        public PurchaseService(IPCSDBContext context)
        {
            _context = context;
        }

        private async Task<string> GeneratePurchaseCodeAsync()
        {
            var last = await _context.Set<PurchaseMaster>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .OrderByDescending(p => p.PurchaseId)
                .FirstOrDefaultAsync();
            int nextId = (last == null) ? 1 : last.PurchaseId + 1;
            return $"PUR-{nextId:D5}";
        }

        public async Task<IEnumerable<PurchaseMaster>> GetAllActiveAsync()
        {
            return await _context.Set<PurchaseMaster>()
                .Include(p => p.Supplier)
                .Include(p => p.Branch)
                .Include(p => p.PaymentMethod)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.PurchaseDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseMaster>> GetDeletedListAsync()
        {
            return await _context.Set<PurchaseMaster>()
                .IgnoreQueryFilters()
                .Include(p => p.Supplier)
                .Include(p => p.Branch)
                .Where(p => p.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PurchaseMaster?> GetByIdAsync(int id)
        {
            return await _context.Set<PurchaseMaster>()
                .Include(p => p.Supplier)
                .Include(p => p.Branch)
                .Include(p => p.PurchaseDetails)
                    .ThenInclude(d => d.Product)
                .Include(p => p.Payments) // Include Split Payments
                    .ThenInclude(pay => pay.PaymentMethod)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PurchaseId == id && !p.IsDeleted);
        }

        public async Task<bool> CreatePurchaseAsync(PurchaseMaster purchaseMaster)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                purchaseMaster.PurchaseCode = await GeneratePurchaseCodeAsync();

                if (purchaseMaster.IsShipment)
                {
                    purchaseMaster.ShipmentStatus = "Pending";
                }
                else
                {
                    purchaseMaster.ShipmentStatus = "Received";
                }

                // 1. Save Purchase
                await _context.AddAsync(purchaseMaster);
                await _context.SaveChangesAsync(); 

                // 2. Financial Update: Supplier Due
                if (purchaseMaster.DueAmount > 0)
                {
                    var supplier = await _context.Suppliers.FindAsync(purchaseMaster.SupplierId);
                    if (supplier != null)
                    {
                        supplier.CurrentDue += purchaseMaster.DueAmount;
                        _context.Suppliers.Update(supplier);
                    }
                }

                // 3. Financial Update: Daily Transaction Summary (Today)
                var today = DateTime.Now.Date;
                var dailySummary = await _context.DailyTransactionSummaries.FirstOrDefaultAsync(d => d.TransactionDate == today);
                if (dailySummary != null)
                {
                    dailySummary.TotalPurchase += purchaseMaster.TotalAmount;
                    
                    // Update specific buckets if needed, or we just track total purchase here
                    _context.DailyTransactionSummaries.Update(dailySummary);
                }

                // 3.1 Update Daily Summary for each Split Payment (Actual Cash Outflow)
                foreach(var payment in purchaseMaster.Payments)
                {
                     var method = await _context.PaymentMethods.FindAsync(payment.PaymentMethodId);
                     if (method != null)
                     {
                         string name = method.MethodName.ToLower();
                         if (name.Contains("cash")) dailySummary.TotalCash -= payment.Amount;
                         else if (name.Contains("bkash")) dailySummary.TotalBkash -= payment.Amount;
                         else if (name.Contains("nagad")) dailySummary.TotalNagad -= payment.Amount;
                         else if (name.Contains("card") || name.Contains("bank")) dailySummary.TotalCard -= payment.Amount;
                     }
                }

                // 4. Inventory Update if NOT Shipment
                if (!purchaseMaster.IsShipment)
                {
                    await UpdateInventoryAsync(purchaseMaster);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error in Purchase: " + ex.Message, ex);
            }
        }

        public async Task<bool> ReceiveShipmentAsync(int purchaseId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var purchaseMaster = await _context.Set<PurchaseMaster>()
                    .Include(p => p.PurchaseDetails)
                    .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

                if (purchaseMaster == null || purchaseMaster.ShipmentStatus != "Pending")
                    return false; // Not found or already received

                purchaseMaster.ShipmentStatus = "Received";
                _context.Update(purchaseMaster);

                // Update Inventory
                await UpdateInventoryAsync(purchaseMaster);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error Receiving Shipment: " + ex.Message, ex);
            }
        }

        private async Task UpdateInventoryAsync(PurchaseMaster purchaseMaster)
        {
            foreach (var detail in purchaseMaster.PurchaseDetails)
            {
                // Validate if Product exists
                var product = await _context.Products.FindAsync(detail.ProductId);
                if (product == null)
                {
                    throw new Exception($"Product with ID {detail.ProductId} not found. Inventory update failed for purchase: {purchaseMaster.PurchaseCode}");
                }

                // 1. Handle Lot/Batch
                var lot = await _context.LotInfos.AsNoTracking().FirstOrDefaultAsync(l => l.ProductId == detail.ProductId && l.LotNumber == detail.BatchNo);
                if (lot == null)
                {
                    lot = new LotInfo
                    {
                        ProductId = detail.ProductId,
                        LotNumber = detail.BatchNo,
                        ExpiryDate = detail.ExpiryDate,
                        PurchasePrice = detail.UnitCostInPcs,
                        IsActive = true
                    };
                    await _context.LotInfos.AddAsync(lot);
                    await _context.SaveChangesAsync(); // Save to get LotId
                }

                var totalQtyPcs = detail.TotalQtyInPcs;

                // 2. Branch Lot Stock
                var branchLotStock = await _context.BranchLotStocks
                    .FirstOrDefaultAsync(b => b.BranchId == purchaseMaster.BranchId && b.ProductId == detail.ProductId && b.LotId == lot.LotId);
                
                if (branchLotStock == null)
                {
                    branchLotStock = new BranchLotStock
                    {
                        BranchId = purchaseMaster.BranchId,
                        ProductId = detail.ProductId,
                        LotId = lot.LotId,
                        CurrentStock = totalQtyPcs
                    };
                    await _context.BranchLotStocks.AddAsync(branchLotStock);
                }
                else
                {
                    branchLotStock.CurrentStock += totalQtyPcs;
                    _context.BranchLotStocks.Update(branchLotStock);
                }

                // 3. Stock Ledger
                var lastLedger = await _context.StockLedgers
                    .AsNoTracking()
                    .Where(l => l.BranchId == purchaseMaster.BranchId && l.ProductId == detail.ProductId && l.LotId == lot.LotId)
                    .OrderByDescending(l => l.LedgerId)
                    .FirstOrDefaultAsync();

                decimal previousBalance = lastLedger?.CurrentBalance ?? 0;

                var ledger = new StockLedger
                {
                    BranchId = purchaseMaster.BranchId,
                    ProductId = detail.ProductId,
                    LotId = lot.LotId,
                    TransactionType = "Purchase",
                    ReferenceNo = purchaseMaster.PurchaseCode,
                    TransactionDate = DateTime.Now,
                    PreviousBalance = previousBalance,
                    QuantityIn = totalQtyPcs,
                    QuantityOut = 0
                };
                await _context.StockLedgers.AddAsync(ledger);

                // 4. Moving Average Cost Logic
                decimal totalValue = (product.CurrentStock * product.CostPrice) + (totalQtyPcs * detail.UnitCostInPcs);
                decimal newTotalStock = product.CurrentStock + totalQtyPcs;
                
                if (newTotalStock > 0)
                {
                    product.CostPrice = Math.Round(totalValue / newTotalStock, 2);
                }
                product.CurrentStock = newTotalStock;
                product.MRP = detail.MRP; // Update MRP as per latest purchase
                
                _context.Products.Update(product);
            }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var p = await _context.Set<PurchaseMaster>().FindAsync(id);
            if (p == null) return false;
            p.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var p = await _context.Set<PurchaseMaster>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.PurchaseId == id);
            if (p == null) return false;
            p.IsDeleted = false;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
