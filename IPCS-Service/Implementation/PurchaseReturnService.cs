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
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IPCSDBContext _context;

        public PurchaseReturnService(IPCSDBContext context)
        {
            _context = context;
        }

        private async Task<string> GenerateReturnNoAsync()
        {
            var last = await _context.PurchaseReturnMasters.IgnoreQueryFilters().OrderByDescending(r => r.ReturnId).FirstOrDefaultAsync();
            int nextId = (last == null) ? 1 : last.ReturnId + 1;
            return $"RET-{nextId:D5}";
        }

        public async Task<IEnumerable<PurchaseReturnMaster>> GetAllActiveAsync(string? search = null)
        {
            var query = _context.PurchaseReturnMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(r => r.ReturnNo.ToLower().Contains(term)
                    || (r.Supplier != null && r.Supplier.SupplierName.ToLower().Contains(term)));
            }

            return await query
                .Include(r => r.Supplier)
                .Include(r => r.Branch)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Server-side paging for the purchase-return list. Same shape and
        /// filter as GetAllActiveAsync, but only one page of rows is loaded.
        /// </summary>
        public async Task<PagedResult<PurchaseReturnMaster>> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.PurchaseReturnMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(r => r.ReturnNo.ToLower().Contains(term)
                    || (r.Supplier != null && r.Supplier.SupplierName.ToLower().Contains(term)));
            }

            query = query
                .Include(r => r.Supplier)
                .Include(r => r.Branch)
                .OrderByDescending(r => r.ReturnDate)
                .ThenByDescending(r => r.ReturnId)
                .AsNoTracking();

            return await query.ToPagedResultAsync(page, pageSize);
        }

        public async Task<PurchaseReturnMaster?> GetByIdAsync(int id)
        {
            return await _context.PurchaseReturnMasters
                .Include(r => r.Supplier)
                .Include(r => r.Branch)
                .Include(r => r.ReturnDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(r => r.ReturnId == id);
        }

        public async Task<bool> CreateReturnAsync(PurchaseReturnMaster returnMaster)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                returnMaster.ReturnNo = await GenerateReturnNoAsync();

                // Guard (new 2026-11-26): a return cannot be created against
                // a deleted purchase. The return would create a new PREF-
                // audit payment while the purchase is deleted, changing the
                // live-payment set between the purchase's delete and
                // restore and breaking the delete/restore reconciliation.
                // Restore the purchase first. (A return with no linked
                // purchase has no invoice to order against.)
                if (returnMaster.PurchaseId.HasValue)
                {
                    var purchase = await _context.PurchaseMasters
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(x => x.PurchaseId == returnMaster.PurchaseId.Value);
                    if (purchase == null)
                        throw new Exception($"Cannot create a purchase return: the purchase #{returnMaster.PurchaseId.Value} does not exist.");
                    if (purchase.IsDeleted)
                        throw new Exception($"Cannot create purchase return {returnMaster.ReturnNo}: the purchase {purchase.PurchaseCode} is deleted. Restore the purchase first, then create the return.");
                }

                foreach (var detail in returnMaster.ReturnDetails)
                {
                    var stock = await _context.BranchLotStocks
                        .FirstOrDefaultAsync(s => s.BranchId == returnMaster.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);

                    if (stock == null) throw new Exception($"Stock record not found for Product ID {detail.ProductId}");

                    if (detail.FromDamagedPool)
                    {
                        if (stock.DamagedStock < detail.Quantity) throw new Exception("Insufficient damaged stock for return.");
                        stock.DamagedStock -= detail.Quantity;
                    }
                    else
                    {
                        if (stock.CurrentStock < detail.Quantity) throw new Exception("Insufficient sellable stock for return.");
                        stock.CurrentStock -= detail.Quantity;

                        // Also update global product stock
                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.CurrentStock -= detail.Quantity;
                            _context.Products.Update(product);
                        }
                    }
                    _context.BranchLotStocks.Update(stock);

                    // Stock Ledger Entry
                    var lastLedger = await _context.StockLedgers
                        .Where(l => l.BranchId == returnMaster.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                        .OrderByDescending(l => l.LedgerId)
                        .FirstOrDefaultAsync();

                    var ledger = new StockLedger
                    {
                        BranchId = returnMaster.BranchId,
                        ProductId = detail.ProductId,
                        LotId = detail.LotId,
                        TransactionType = detail.FromDamagedPool ? "PurchaseReturn(Damaged)" : "PurchaseReturn",
                        ReferenceNo = returnMaster.ReturnNo,
                        TransactionDate = DateTime.Now,
                        PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                        QuantityIn = 0,
                        QuantityOut = detail.Quantity
                    };
                    await _context.StockLedgers.AddAsync(ledger);
                }

                // Financial Update: Supplier Due vs Cash Refund
                if (returnMaster.RefundAmount > 0)
                {
                    if (returnMaster.RefundType == "Adjustment")
                    {
                        var supplier = await _context.Suppliers.FindAsync(returnMaster.SupplierId);
                        if (supplier != null)
                        {
                            // Reduce Due. If Due is 0, it becomes Negative (Advance to Supplier)
                            supplier.CurrentDue -= returnMaster.RefundAmount;
                            _context.Suppliers.Update(supplier);
                        }
                    }
                    else if (returnMaster.RefundType == "Cash")
                    {
                        bool isDigital = false;
                        if (returnMaster.PaymentMethodId.HasValue)
                        {
                            var method = await _context.PaymentMethods.FindAsync(returnMaster.PaymentMethodId.Value);
                            if (method != null && method.IsDigital) isDigital = true;
                        }

                        // Update Daily Summary Balance
                        var today = DateTime.Now.Date;
                        var summary = await _context.DailyTransactionSummaries
                            .FirstOrDefaultAsync(d => d.TransactionDate == today && d.BranchId == returnMaster.BranchId);

                        if (summary != null)
                        {
                            // ONLY increase physical cash if it's not a digital refund (e.g. Supplier gave physical cash)
                            if (!isDigital)
                            {
                                summary.TotalCash += returnMaster.RefundAmount;
                            }
                            _context.DailyTransactionSummaries.Update(summary);
                        }

                        // Create Audit Trail in InvoicePayments
                        var payment = new InvoicePayment
                        {
                            PurchaseId = returnMaster.PurchaseId,
                            PaymentDate = DateTime.Now,
                            Amount = returnMaster.RefundAmount, // Positive because money is COMING BACK from supplier
                            PaymentMethodId = returnMaster.PaymentMethodId ?? 1,
                            TransactionType = "PurchaseReturnRefund",
                            TransactionNo = "PREF-" + returnMaster.ReturnNo,
                            Remarks = "Refund from Supplier for Return No: " + returnMaster.ReturnNo
                        };
                        await _context.InvoicePayments.AddAsync(payment);
                    }
                }

                // Daily Transaction Summary (Inventory Value adjustment)
                var todayDate = DateTime.Now.Date;
                var summaryInv = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(d => d.TransactionDate == todayDate && d.BranchId == returnMaster.BranchId);

                if (summaryInv != null)
                {
                    // Purchase Return decreases the total purchase investment of the day
                    summaryInv.TotalPurchase -= returnMaster.TotalAmount;
                    if (summaryInv.TotalPurchase < 0) summaryInv.TotalPurchase = 0;
                    _context.DailyTransactionSummaries.Update(summaryInv);
                }

                await _context.PurchaseReturnMasters.AddAsync(returnMaster);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Purchase Return Failed: " + ex.Message);
            }
        }

        // Header-only edit: Reason and ReturnDate only. Quantities, amounts,
        // refund type and lines are never touched, so stock and books
        // cannot be corrupted by this update.
        public async Task<bool> UpdateHeaderAsync(int id, DocumentHeaderUpdateDTO dto)
        {
            var ret = await _context.PurchaseReturnMasters.FindAsync(id);
            if (ret == null) return false;

            if (dto.Reason != null) ret.Reason = dto.Reason;
            if (dto.DocumentDate.HasValue) ret.ReturnDate = dto.DocumentDate.Value;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var r = await _context.PurchaseReturnMasters
                    .Include(x => x.ReturnDetails)
                    .FirstOrDefaultAsync(x => x.ReturnId == id);
                if (r == null) return false;

                // Ordering guard (R1): a return cannot be deleted while
                // its purchase is deleted. Deleting it would hard-remove
                // its refund audit payment (PREF-), so a later restore of
                // the purchase could no longer see the refund and the
                // daily totals would end up off by the refund amount.
                // Restore the purchase first. (A return with no linked
                // purchase has no invoice to order against, so the guard
                // does not apply to it.)
                if (r.PurchaseId.HasValue)
                {
                    var purchase = await _context.PurchaseMasters
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(x => x.PurchaseId == r.PurchaseId.Value);
                    if (purchase != null && purchase.IsDeleted)
                        throw new Exception($"Cannot delete purchase return {r.ReturnNo}: the purchase {purchase.PurchaseCode} is deleted. Restore the purchase first, then delete the return.");
                }

                // 1. Reverse stock: add the returned quantities back to their lots.
                foreach (var detail in r.ReturnDetails)
                {
                    var stock = await _context.BranchLotStocks
                        .FirstOrDefaultAsync(s => s.BranchId == r.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);
                    if (stock == null)
                        throw new Exception($"Cannot delete return {r.ReturnNo}: stock record not found for Product ID {detail.ProductId}.");

                    if (detail.FromDamagedPool)
                    {
                        stock.DamagedStock += detail.Quantity;
                    }
                    else
                    {
                        stock.CurrentStock += detail.Quantity;

                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.CurrentStock += detail.Quantity;
                            _context.Products.Update(product);
                        }
                    }
                    _context.BranchLotStocks.Update(stock);

                    var lastLedger = await _context.StockLedgers
                        .Where(l => l.BranchId == r.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                        .OrderByDescending(l => l.LedgerId)
                        .FirstOrDefaultAsync();
                    await _context.StockLedgers.AddAsync(new StockLedger
                    {
                        BranchId = r.BranchId,
                        ProductId = detail.ProductId,
                        LotId = detail.LotId,
                        TransactionType = "PurchaseReturn Delete",
                        ReferenceNo = r.ReturnNo,
                        TransactionDate = DateTime.Now,
                        PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                        QuantityIn = detail.Quantity,
                        QuantityOut = 0
                    });
                }

                // 2. Reverse the financial effects (mirror of CreateReturnAsync).
                if (r.RefundAmount > 0)
                {
                    if (r.RefundType == "Adjustment")
                    {
                        var supplier = await _context.Suppliers.FindAsync(r.SupplierId);
                        if (supplier != null)
                        {
                            supplier.CurrentDue += r.RefundAmount;
                            _context.Suppliers.Update(supplier);
                        }
                    }
                    else if (r.RefundType == "Cash")
                    {
                        bool isDigital = false;
                        if (r.PaymentMethodId.HasValue)
                        {
                            var method = await _context.PaymentMethods.FindAsync(r.PaymentMethodId.Value);
                            if (method != null && method.IsDigital) isDigital = true;
                        }

                        if (!isDigital)
                        {
                            var summaryCash = await _context.DailyTransactionSummaries
                                .FirstOrDefaultAsync(d => d.TransactionDate == r.ReturnDate.Date && d.BranchId == r.BranchId);
                            if (summaryCash != null)
                            {
                                summaryCash.TotalCash -= r.RefundAmount;
                                _context.DailyTransactionSummaries.Update(summaryCash);
                            }
                        }

                        // Remove the refund audit payment created for this return.
                        var refundPayment = await _context.InvoicePayments
                            .FirstOrDefaultAsync(p => p.TransactionNo == "PREF-" + r.ReturnNo);
                        if (refundPayment != null)
                        {
                            _context.InvoicePayments.Remove(refundPayment);
                        }
                    }
                }

                // 3. Reverse the DailySummary inventory adjustment.
                var summary = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(d => d.TransactionDate == r.ReturnDate.Date && d.BranchId == r.BranchId);
                if (summary != null)
                {
                    summary.TotalPurchase += r.TotalAmount;
                    _context.DailyTransactionSummaries.Update(summary);
                }

                r.IsDeleted = true;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Purchase Return Delete Failed: " + ex.Message);
            }
        }

        public async Task<IEnumerable<PurchaseReturnMaster>> GetDeletedListAsync()
        {
            return await _context.PurchaseReturnMasters
                .IgnoreQueryFilters()
                .Include(r => r.Supplier)
                .Include(r => r.Branch)
                .Where(r => r.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        // Restore re-applies the creation effects exactly as creation did
        // (and exactly undoes what SoftDeleteAsync reversed), in one
        // transaction. Guard: if the returned goods are no longer in stock
        // (sold on after the return was deleted), the restore is rejected
        // with a clear error instead of driving stock negative.
        public async Task<bool> RestoreAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var r = await _context.PurchaseReturnMasters
                    .Include(x => x.ReturnDetails)
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.ReturnId == id);
                if (r == null) return false;
                if (!r.IsDeleted) return false;

                // Ordering guard (R3): a return cannot be restored while
                // its purchase is deleted — the purchase delete already
                // unwound the purchase, so re-applying the return alone
                // would double-apply the refund. Restore the purchase
                // first. (A return with no linked purchase has no invoice
                // to order against, so the guard does not apply to it.)
                if (r.PurchaseId.HasValue)
                {
                    var purchase = await _context.PurchaseMasters
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(x => x.PurchaseId == r.PurchaseId.Value);
                    if (purchase != null && purchase.IsDeleted)
                        throw new Exception($"Cannot restore purchase return {r.ReturnNo}: the purchase {purchase.PurchaseCode} is deleted. Restore the purchase first, then restore the return.");
                }

                // 1. Re-apply stock: remove the returned quantities again.
                foreach (var detail in r.ReturnDetails)
                {
                    var stock = await _context.BranchLotStocks
                        .FirstOrDefaultAsync(s => s.BranchId == r.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);
                    if (stock == null)
                        throw new Exception($"Cannot restore return {r.ReturnNo}: stock record not found for Product ID {detail.ProductId}.");

                    if (detail.FromDamagedPool)
                    {
                        if (stock.DamagedStock < detail.Quantity)
                            throw new Exception($"Cannot restore return {r.ReturnNo}: only {stock.DamagedStock} damaged units of Product ID {detail.ProductId} remain (needs {detail.Quantity}).");
                        stock.DamagedStock -= detail.Quantity;
                    }
                    else
                    {
                        if (stock.CurrentStock < detail.Quantity)
                            throw new Exception($"Cannot restore return {r.ReturnNo}: only {stock.CurrentStock} units of Product ID {detail.ProductId} remain in stock (needs {detail.Quantity}). The returned goods were sold after the return was deleted.");
                        stock.CurrentStock -= detail.Quantity;

                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.CurrentStock -= detail.Quantity;
                            _context.Products.Update(product);
                        }
                    }
                    _context.BranchLotStocks.Update(stock);

                    var lastLedger = await _context.StockLedgers
                        .Where(l => l.BranchId == r.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                        .OrderByDescending(l => l.LedgerId)
                        .FirstOrDefaultAsync();
                    await _context.StockLedgers.AddAsync(new StockLedger
                    {
                        BranchId = r.BranchId,
                        ProductId = detail.ProductId,
                        LotId = detail.LotId,
                        TransactionType = detail.FromDamagedPool ? "PurchaseReturn Restore(Damaged)" : "PurchaseReturn Restore",
                        ReferenceNo = r.ReturnNo,
                        TransactionDate = DateTime.Now,
                        PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                        QuantityIn = 0,
                        QuantityOut = detail.Quantity
                    });
                }

                // 2. Re-apply the financial effects (mirror of CreateReturnAsync).
                if (r.RefundAmount > 0)
                {
                    if (r.RefundType == "Adjustment")
                    {
                        var supplier = await _context.Suppliers.FindAsync(r.SupplierId);
                        if (supplier != null)
                        {
                            supplier.CurrentDue -= r.RefundAmount;
                            _context.Suppliers.Update(supplier);
                        }
                    }
                    else if (r.RefundType == "Cash")
                    {
                        bool isDigital = false;
                        if (r.PaymentMethodId.HasValue)
                        {
                            var method = await _context.PaymentMethods.FindAsync(r.PaymentMethodId.Value);
                            if (method != null && method.IsDigital) isDigital = true;
                        }

                        if (!isDigital)
                        {
                            var summaryCash = await GetOrCreateSummaryAsync(r.ReturnDate.Date, r.BranchId);
                            summaryCash.TotalCash += r.RefundAmount;
                            _context.DailyTransactionSummaries.Update(summaryCash);
                        }

                        // Re-create the refund audit payment (delete removed
                        // it); skip if one already exists for this return.
                        var existing = await _context.InvoicePayments.IgnoreQueryFilters()
                            .FirstOrDefaultAsync(p => p.TransactionNo == "PREF-" + r.ReturnNo);
                        if (existing == null)
                        {
                            await _context.InvoicePayments.AddAsync(new InvoicePayment
                            {
                                PurchaseId = r.PurchaseId,
                                PaymentDate = DateTime.Now,
                                Amount = r.RefundAmount, // Positive because money is COMING BACK from supplier
                                PaymentMethodId = r.PaymentMethodId ?? 1,
                                TransactionType = "PurchaseReturnRefund",
                                TransactionNo = "PREF-" + r.ReturnNo,
                                Remarks = "Refund from Supplier for Return No: " + r.ReturnNo
                            });
                        }
                        else if (existing.IsDeleted)
                        {
                            existing.IsDeleted = false;
                            _context.InvoicePayments.Update(existing);
                        }
                    }
                }

                // 3. Re-apply the DailySummary inventory adjustment.
                var summary = await GetOrCreateSummaryAsync(r.ReturnDate.Date, r.BranchId);
                summary.TotalPurchase -= r.TotalAmount;
                if (summary.TotalPurchase < 0) summary.TotalPurchase = 0;
                _context.DailyTransactionSummaries.Update(summary);

                r.IsDeleted = false;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Purchase Return Restore Failed: " + ex.Message);
            }
        }

        private async Task<DailyTransactionSummary> GetOrCreateSummaryAsync(DateTime date, int branchId)
        {
            var summary = await _context.DailyTransactionSummaries
                .FirstOrDefaultAsync(d => d.TransactionDate == date && d.BranchId == branchId);
            if (summary == null)
            {
                summary = new DailyTransactionSummary
                {
                    TransactionDate = date,
                    BranchId = branchId,
                    OpeningBalance = 0
                };
                await _context.DailyTransactionSummaries.AddAsync(summary);
            }
            return summary;
        }
    }
}
