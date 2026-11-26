using IPCS_Model.Entities;
using IPCS_Model.DTOs;
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

                // 3. Financial Update: Daily Transaction Summary (Today, per branch)
                // Create the row if missing (same as SalesService) — otherwise the
                // payment-bucket update in 3.1 throws NullReferenceException when
                // this is the first transaction of the day on a fresh database.
                var today = DateTime.Now.Date;
                var dailySummary = await _context.DailyTransactionSummaries.FirstOrDefaultAsync(d => d.TransactionDate == today && d.BranchId == purchaseMaster.BranchId);
                if (dailySummary == null)
                {
                    dailySummary = new DailyTransactionSummary
                    {
                        TransactionDate = today,
                        BranchId = purchaseMaster.BranchId,
                        OpeningBalance = 0
                    };
                    await _context.DailyTransactionSummaries.AddAsync(dailySummary);
                }

                dailySummary.TotalPurchase += purchaseMaster.TotalAmount;

                // 3.1 Update Daily Summary for each Split Payment (Actual Cash Outflow)
                foreach(var payment in purchaseMaster.Payments ?? Enumerable.Empty<InvoicePayment>())
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var p = await _context.Set<PurchaseMaster>()
                    .Include(x => x.PurchaseDetails)
                    .Include(x => x.Payments)
                    .FirstOrDefaultAsync(x => x.PurchaseId == id);
                if (p == null) return false;

                // Ordering guard (R4): a purchase with a deleted return
                // cannot be deleted — restore the return(s) first, so the
                // return's refund audit payment (PREF-) is visible again
                // when the purchase is restored later. Otherwise the
                // restore would miss the refund and the daily totals
                // would end up off by the refund amount.
                var deletedReturns = await _context.PurchaseReturnMasters
                    .IgnoreQueryFilters()
                    .Where(x => x.PurchaseId == id && x.IsDeleted)
                    .Select(x => x.ReturnNo)
                    .ToListAsync();
                if (deletedReturns.Count > 0)
                    throw new Exception(DeletedReturnsMessage("delete", p.PurchaseCode, deletedReturns));

                // 1. Reverse stock (only if the goods were received into inventory).
                if (!p.IsShipment || p.ShipmentStatus == "Received")
                {
                    // Guard first: every lot must still hold the purchased
                    // quantity. If any of it was already sold/used, refuse
                    // instead of driving stock negative.
                    var reversals = new List<(PurchaseDetails Detail, LotInfo Lot, BranchLotStock Stock)>();
                    foreach (var detail in p.PurchaseDetails)
                    {
                        var lot = await _context.LotInfos.AsNoTracking()
                            .FirstOrDefaultAsync(l => l.ProductId == detail.ProductId && l.LotNumber == detail.BatchNo);
                        if (lot == null)
                            throw new Exception($"Cannot delete purchase {p.PurchaseCode}: lot {detail.BatchNo} for Product ID {detail.ProductId} no longer exists.");

                        var stock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == p.BranchId && s.ProductId == detail.ProductId && s.LotId == lot.LotId);
                        if (stock == null || stock.CurrentStock < detail.TotalQtyInPcs)
                            throw new Exception($"Cannot delete purchase {p.PurchaseCode}: stock for Product ID {detail.ProductId} (lot {detail.BatchNo}) has already been sold or used.");

                        reversals.Add((detail, lot, stock));
                    }

                    foreach (var (detail, lot, stock) in reversals)
                    {
                        stock.CurrentStock -= detail.TotalQtyInPcs;
                        _context.BranchLotStocks.Update(stock);

                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.CurrentStock -= detail.TotalQtyInPcs;
                            _context.Products.Update(product);
                        }

                        var lastLedger = await _context.StockLedgers.AsNoTracking()
                            .Where(l => l.BranchId == p.BranchId && l.ProductId == detail.ProductId && l.LotId == lot.LotId)
                            .OrderByDescending(l => l.LedgerId)
                            .FirstOrDefaultAsync();
                        await _context.StockLedgers.AddAsync(new StockLedger
                        {
                            BranchId = p.BranchId,
                            ProductId = detail.ProductId,
                            LotId = lot.LotId,
                            TransactionType = "Purchase Delete",
                            ReferenceNo = p.PurchaseCode,
                            TransactionDate = DateTime.Now,
                            PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                            QuantityIn = 0,
                            QuantityOut = detail.TotalQtyInPcs
                        });
                    }
                }

                // 2. Reverse supplier due (only what is still owed).
                if (p.DueAmount > 0)
                {
                    var supplier = await _context.Suppliers.FindAsync(p.SupplierId);
                    if (supplier != null)
                    {
                        supplier.CurrentDue -= p.DueAmount;
                        _context.Suppliers.Update(supplier);
                    }
                }

                // 3. Reverse the DailySummary effects (mirror of CreatePurchaseAsync).
                // The summary row is the one for the purchase date + branch.
                var summary = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(d => d.TransactionDate == p.PurchaseDate.Date && d.BranchId == p.BranchId);
                if (summary != null)
                {
                    summary.TotalPurchase -= p.TotalAmount;
                    if (summary.TotalPurchase < 0) summary.TotalPurchase = 0;
                    // The purchase's payments (inline at creation and any later
                    // split payments) are unwound together with the voided
                    // purchase; the payment rows themselves are kept as records.
                    foreach (var payment in p.Payments ?? Enumerable.Empty<InvoicePayment>())
                    {
                        var method = await _context.PaymentMethods.FindAsync(payment.PaymentMethodId);
                        if (method != null)
                        {
                            string name = method.MethodName.ToLower();
                            if (name.Contains("cash")) summary.TotalCash += payment.Amount;
                            else if (name.Contains("bkash")) summary.TotalBkash += payment.Amount;
                            else if (name.Contains("nagad")) summary.TotalNagad += payment.Amount;
                            else if (name.Contains("card") || name.Contains("bank")) summary.TotalCard += payment.Amount;
                        }
                    }
                    _context.DailyTransactionSummaries.Update(summary);
                }

                p.IsDeleted = true;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Purchase Delete Failed: " + ex.Message, ex);
            }
        }

        // Header-only edit: Remarks and PurchaseDate only. Quantities, amounts,
        // supplier, branch and lines are never touched, so stock and books
        // cannot be corrupted by this update.
        public async Task<bool> UpdateHeaderAsync(int id, DocumentHeaderUpdateDTO dto)
        {
            var purchase = await _context.PurchaseMasters.FindAsync(id);
            if (purchase == null) return false;

            if (dto.Remarks != null) purchase.Remarks = dto.Remarks;
            if (dto.DocumentDate.HasValue) purchase.PurchaseDate = dto.DocumentDate.Value;
            return await _context.SaveChangesAsync() > 0;
        }

        // Restore re-applies the creation effects exactly as creation did
        // (and exactly undoes what SoftDeleteAsync reversed), in one
        // transaction. Stock is added back (lot re-created if it was
        // hard-deleted meanwhile); supplier due, TotalPurchase and the
        // payment buckets are re-applied. CostPrice/MRP are NOT recomputed:
        // delete left them untouched, so restore must too, or the moving
        // average cost would drift. Only payments that are still live are
        // counted — a payment soft-deleted earlier already had its bucket
        // effect reversed by its own delete, so it must not be counted here.
        public async Task<bool> RestoreAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var p = await _context.Set<PurchaseMaster>()
                    .Include(x => x.PurchaseDetails)
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.PurchaseId == id);
                if (p == null) return false;
                if (!p.IsDeleted) return false;

                // Ordering guard (R2): a purchase with a deleted return
                // cannot be restored — restore the return(s) first, so
                // the return's refund audit payment (PREF-) is visible
                // again and the restore re-applies the same payment set
                // the delete unwound. Otherwise the daily totals would
                // end up off by the refund amount.
                var deletedReturns = await _context.PurchaseReturnMasters
                    .IgnoreQueryFilters()
                    .Where(x => x.PurchaseId == id && x.IsDeleted)
                    .Select(x => x.ReturnNo)
                    .ToListAsync();
                if (deletedReturns.Count > 0)
                    throw new Exception(DeletedReturnsMessage("restore", p.PurchaseCode, deletedReturns));

                // 1. Re-apply stock (only if the goods were received into inventory).
                if (!p.IsShipment || p.ShipmentStatus == "Received")
                {
                    foreach (var detail in p.PurchaseDetails)
                    {
                        var lot = await _context.LotInfos.AsNoTracking()
                            .FirstOrDefaultAsync(l => l.ProductId == detail.ProductId && l.LotNumber == detail.BatchNo);
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

                        var stock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == p.BranchId && s.ProductId == detail.ProductId && s.LotId == lot.LotId);
                        if (stock == null)
                        {
                            stock = new BranchLotStock
                            {
                                BranchId = p.BranchId,
                                ProductId = detail.ProductId,
                                LotId = lot.LotId,
                                CurrentStock = detail.TotalQtyInPcs
                            };
                            await _context.BranchLotStocks.AddAsync(stock);
                        }
                        else
                        {
                            stock.CurrentStock += detail.TotalQtyInPcs;
                            _context.BranchLotStocks.Update(stock);
                        }

                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product == null)
                            throw new Exception($"Cannot restore purchase {p.PurchaseCode}: Product ID {detail.ProductId} no longer exists.");
                        product.CurrentStock += detail.TotalQtyInPcs;
                        _context.Products.Update(product);

                        var lastLedger = await _context.StockLedgers.AsNoTracking()
                            .Where(l => l.BranchId == p.BranchId && l.ProductId == detail.ProductId && l.LotId == lot.LotId)
                            .OrderByDescending(l => l.LedgerId)
                            .FirstOrDefaultAsync();
                        await _context.StockLedgers.AddAsync(new StockLedger
                        {
                            BranchId = p.BranchId,
                            ProductId = detail.ProductId,
                            LotId = lot.LotId,
                            TransactionType = "Purchase Restore",
                            ReferenceNo = p.PurchaseCode,
                            TransactionDate = DateTime.Now,
                            PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                            QuantityIn = detail.TotalQtyInPcs,
                            QuantityOut = 0
                        });
                    }
                }

                // 2. Re-apply supplier due (only what is still owed).
                if (p.DueAmount > 0)
                {
                    var supplier = await _context.Suppliers.FindAsync(p.SupplierId);
                    if (supplier != null)
                    {
                        supplier.CurrentDue += p.DueAmount;
                        _context.Suppliers.Update(supplier);
                    }
                }

                // 3. Re-apply the DailySummary effects. The summary row is the
                // one for the purchase date + branch (the same row delete
                // reversed); create it if it is missing.
                var summary = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(d => d.TransactionDate == p.PurchaseDate.Date && d.BranchId == p.BranchId);
                if (summary == null)
                {
                    summary = new DailyTransactionSummary
                    {
                        TransactionDate = p.PurchaseDate.Date,
                        BranchId = p.BranchId,
                        OpeningBalance = 0
                    };
                    await _context.DailyTransactionSummaries.AddAsync(summary);
                }

                summary.TotalPurchase += p.TotalAmount;

                // Only live payments: soft-deleted payments were already
                // unwound by their own delete and must not be counted again.
                var payments = await _context.InvoicePayments
                    .Where(pay => pay.PurchaseId == id)
                    .ToListAsync();
                foreach (var payment in payments)
                {
                    var method = await _context.PaymentMethods.FindAsync(payment.PaymentMethodId);
                    if (method != null)
                    {
                        string name = method.MethodName.ToLower();
                        if (name.Contains("cash")) summary.TotalCash -= payment.Amount;
                        else if (name.Contains("bkash")) summary.TotalBkash -= payment.Amount;
                        else if (name.Contains("nagad")) summary.TotalNagad -= payment.Amount;
                        else if (name.Contains("card") || name.Contains("bank")) summary.TotalCard -= payment.Amount;
                    }
                }
                _context.DailyTransactionSummaries.Update(summary);

                p.IsDeleted = false;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Purchase Restore Failed: " + ex.Message, ex);
            }
        }

        // Shared wording for the R2/R4 ordering guards: a purchase cannot
        // be deleted or restored while one of its returns is deleted.
        private static string DeletedReturnsMessage(string action, string? purchaseCode, List<string> returnNos)
        {
            if (returnNos.Count == 1)
                return $"Cannot {action} purchase {purchaseCode}: purchase return {returnNos[0]} is deleted. Restore the return first, then {action} the purchase.";
            return $"Cannot {action} purchase {purchaseCode}: purchase returns {string.Join(", ", returnNos)} are deleted. Restore the returns first, then {action} the purchase.";
        }
    }
}
