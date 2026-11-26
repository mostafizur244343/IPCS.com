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
    public class SalesReturnService : ISalesReturnService
    {
        private readonly IPCSDBContext _context;

        public SalesReturnService(IPCSDBContext context)
        {
            _context = context;
        }

        private async Task<string> GenerateReturnNoAsync()
        {
            var last = await _context.SalesReturnMasters.IgnoreQueryFilters().OrderByDescending(r => r.ReturnId).FirstOrDefaultAsync();
            int nextId = (last == null) ? 1 : last.ReturnId + 1;
            return $"SRET-{nextId:D5}";
        }

        public async Task<IEnumerable<SalesReturnMaster>> GetAllActiveAsync()
        {
            return await _context.SalesReturnMasters
                .Include(r => r.Customer)
                .Include(r => r.Branch)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SalesReturnMaster?> GetByIdAsync(int id)
        {
            return await _context.SalesReturnMasters
                .Include(r => r.Customer)
                .Include(r => r.Branch)
                .Include(r => r.ReturnDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(r => r.ReturnId == id);
        }

        public async Task<bool> CreateReturnAsync(SalesReturnMaster returnMaster)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                returnMaster.ReturnNo = await GenerateReturnNoAsync();
                decimal totalCostSaved = 0;

                foreach (var detail in returnMaster.ReturnDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null) continue;

                    // 0. Validation against original invoice if SalesId is provided
                    if (returnMaster.SalesId.HasValue)
                    {
                        var originalDetail = await _context.SalesDetails
                            .FirstOrDefaultAsync(d => d.SalesId == returnMaster.SalesId && d.ProductId == detail.ProductId && d.LotId == detail.LotId);

                        if (originalDetail == null)
                            throw new Exception($"Product ID {detail.ProductId} with Lot {detail.LotId} was not part of original Invoice No.");

                        if (detail.Quantity > originalDetail.Quantity)
                            throw new Exception($"Return quantity ({detail.Quantity}) cannot exceed sold quantity ({originalDetail.Quantity}) for Product ID {detail.ProductId}.");

                        // Ensure refund unit price is not more than what was paid (Net Price after discount)
                        decimal paidPrice = originalDetail.UnitPrice - originalDetail.DiscountPerUnit;
                        if (detail.UnitPrice > paidPrice)
                            detail.UnitPrice = paidPrice; // Cap the return price to what was paid
                    }

                    // Skip stock replenishment for Services
                    if (!product.IsService)
                    {
                        // 1. Replenish Stock in specific Lot
                        var stock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == returnMaster.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);

                        if (stock == null)
                        {
                            stock = new BranchLotStock { BranchId = returnMaster.BranchId, ProductId = detail.ProductId, LotId = detail.LotId, CurrentStock = detail.Quantity };
                            await _context.BranchLotStocks.AddAsync(stock);
                        }
                        else
                        {
                            stock.CurrentStock += detail.Quantity;
                            _context.BranchLotStocks.Update(stock);
                        }

                        // 2. Global Product Stock Increase
                        product.CurrentStock += detail.Quantity;
                        _context.Products.Update(product);

                        // 4. Ledger Entry
                        var lastLedger = await _context.StockLedgers
                            .Where(l => l.BranchId == returnMaster.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                            .OrderByDescending(l => l.LedgerId)
                            .FirstOrDefaultAsync();

                        var ledger = new StockLedger
                        {
                            BranchId = returnMaster.BranchId, ProductId = detail.ProductId, LotId = detail.LotId,
                            TransactionType = "SalesReturn", ReferenceNo = returnMaster.ReturnNo, TransactionDate = DateTime.Now,
                            PreviousBalance = lastLedger?.CurrentBalance ?? 0, QuantityIn = detail.Quantity, QuantityOut = 0
                        };
                        await _context.StockLedgers.AddAsync(ledger);

                        // 3. Profit Adjustment Calculation (using exact Lot cost) - ONLY FOR PRODUCTS
                        var lot = await _context.LotInfos.FindAsync(detail.LotId);
                        decimal costPrice = lot?.PurchasePrice ?? 0;
                        totalCostSaved += (costPrice * detail.Quantity);
                    }
                }

                // 5. Financial Adjustments
                var today = DateTime.Now.Date;
                var summary = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(d => d.TransactionDate == today && d.BranchId == returnMaster.BranchId);

                if (returnMaster.RefundType == "Adjustment")
                {
                    var customer = await _context.Customers.FindAsync(returnMaster.CustomerId);
                    // Split the refund: first offset what the customer still
                    // owes (due); the excess is money the customer already
                    // paid, so credit it to their advance balance instead of
                    // driving CurrentDue negative.
                    decimal fromDue = 0;
                    decimal toAdvance = returnMaster.RefundAmount;
                    if (customer != null)
                    {
                        fromDue = Math.Min(customer.CurrentDue, returnMaster.RefundAmount);
                        toAdvance = returnMaster.RefundAmount - fromDue;
                        customer.CurrentDue -= fromDue;
                        if (toAdvance > 0) customer.AdvanceBalance += toAdvance;
                        _context.Customers.Update(customer);
                    }

                    if (summary != null)
                    {
                        // The due part comes out of due sales; the cash-backed
                        // part comes out of cash sales (that revenue is
                        // returned to the customer).
                        summary.TotalDueSales -= fromDue;
                        if (summary.TotalDueSales < 0) summary.TotalDueSales = 0;
                        summary.TotalCashSales -= toAdvance;
                        if (summary.TotalCashSales < 0) summary.TotalCashSales = 0;
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

                    if (summary != null)
                    {
                        // Revenue is lost regardless of method
                        summary.TotalCashSales -= returnMaster.RefundAmount;
                        if (summary.TotalCashSales < 0) summary.TotalCashSales = 0;

                        // ONLY decrease physical cash if it's not a digital refund
                        if (!isDigital)
                        {
                            summary.TotalCash -= returnMaster.RefundAmount;
                        }
                        
                        _context.DailyTransactionSummaries.Update(summary);
                    }

                    // Create Audit Trail in InvoicePayments
                    var payment = new InvoicePayment
                    {
                        SaleId = returnMaster.SalesId,
                        PaymentDate = DateTime.Now,
                        Amount = -returnMaster.RefundAmount, // Negative to represent refund
                        PaymentMethodId = returnMaster.PaymentMethodId ?? 1, // Default to Cash if null
                        TransactionType = "Refund",
                        TransactionNo = "REF-" + returnMaster.ReturnNo,
                        Remarks = "Refund for Return No: " + returnMaster.ReturnNo
                    };
                    await _context.InvoicePayments.AddAsync(payment);
                }
                else if (returnMaster.RefundType == "Credit" || returnMaster.RefundType == "Wallet")
                {
                    var customer = await _context.Customers.FindAsync(returnMaster.CustomerId);
                    if (customer != null)
                    {
                        customer.AdvanceBalance += returnMaster.RefundAmount;
                        _context.Customers.Update(customer);
                    }
                }

                // 6. Adjust Daily Profit & Sales
                if (summary != null)
                {
                    // Profit lost = Revenue Lost - Cost Saved
                    decimal profitLost = returnMaster.TotalAmount - totalCostSaved;
                    summary.NetProfit -= profitLost;
                    
                    // Note: We don't necessarily have a "Total Sales" column to decrease, 
                    // but we adjusted CashSales/DueSales above.
                    _context.DailyTransactionSummaries.Update(summary);
                }

                await _context.SalesReturnMasters.AddAsync(returnMaster);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Sales Return Failed: " + ex.Message);
            }
        }

        // Header-only edit: Reason and ReturnDate only. Quantities, amounts,
        // refund type and lines are never touched, so stock and books
        // cannot be corrupted by this update.
        public async Task<bool> UpdateHeaderAsync(int id, DocumentHeaderUpdateDTO dto)
        {
            var ret = await _context.SalesReturnMasters.FindAsync(id);
            if (ret == null) return false;

            if (dto.Reason != null) ret.Reason = dto.Reason;
            if (dto.DocumentDate.HasValue) ret.ReturnDate = dto.DocumentDate.Value;
            return await _context.SaveChangesAsync() > 0;
        }

        // NOTE: this delete DOES reverse the return's effects (stock,
        // customer balances, DailySummary, audit payment). An earlier version
        // only flipped IsDeleted, which left the books counting a return
        // that no longer existed; restore could then not re-apply the
        // effects without double-counting. Delete and restore are now exact
        // inverses of each other, like Purchase/Sales/PurchaseReturn.
        public async Task<bool> SoftDeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var r = await _context.SalesReturnMasters
                    .Include(x => x.ReturnDetails)
                    .FirstOrDefaultAsync(x => x.ReturnId == id);
                if (r == null) return false;

                // Ordering guard (R1): a return cannot be deleted while
                // its sale is deleted. Deleting it would hard-remove its
                // refund audit payment (REF-), so a later restore of the
                // sale could no longer see the refund and the daily totals
                // would end up off by the refund amount. Restore the sale
                // first. (A return with no linked sale has no invoice to
                // order against, so the guard does not apply to it.)
                if (r.SalesId.HasValue)
                {
                    var sale = await _context.SalesMasters
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(x => x.SalesId == r.SalesId.Value);
                    if (sale != null && sale.IsDeleted)
                        throw new Exception($"Cannot delete sales return {r.ReturnNo}: the sale {sale.InvoiceNo} is deleted. Restore the sale first, then delete the return.");
                }

                // 1. Reverse stock: remove the replenished quantities again.
                // Guard: the returned goods may have been sold on since.
                foreach (var detail in r.ReturnDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null && !product.IsService)
                    {
                        var stock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == r.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);
                        if (stock == null)
                            throw new Exception($"Cannot delete return {r.ReturnNo}: stock record not found for Product ID {detail.ProductId}.");
                        if (stock.CurrentStock < detail.Quantity)
                            throw new Exception($"Cannot delete return {r.ReturnNo}: only {stock.CurrentStock} units of Product ID {detail.ProductId} remain in stock (needs {detail.Quantity}). The returned goods were sold after the return.");
                        stock.CurrentStock -= detail.Quantity;
                        _context.BranchLotStocks.Update(stock);

                        product.CurrentStock -= detail.Quantity;
                        _context.Products.Update(product);

                        var lastLedger = await _context.StockLedgers
                            .Where(l => l.BranchId == r.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                            .OrderByDescending(l => l.LedgerId)
                            .FirstOrDefaultAsync();
                        await _context.StockLedgers.AddAsync(new StockLedger
                        {
                            BranchId = r.BranchId,
                            ProductId = detail.ProductId,
                            LotId = detail.LotId,
                            TransactionType = "SalesReturn Delete",
                            ReferenceNo = r.ReturnNo,
                            TransactionDate = DateTime.Now,
                            PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                            QuantityIn = 0,
                            QuantityOut = detail.Quantity
                        });
                    }
                }

                // 2. Reverse the financial effects (mirror of CreateReturnAsync).
                // The Adjustment split (due vs advance) is not stored on the
                // record, so it is approximated the same way creation split
                // it: take the refund out of the advance balance first, the
                // rest out of the due. Exact when the advance is still
                // intact; never drives either balance negative.
                var summary = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(d => d.TransactionDate == r.ReturnDate.Date && d.BranchId == r.BranchId);

                decimal totalCostSaved = 0;
                foreach (var detail in r.ReturnDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null && !product.IsService)
                    {
                        var lot = await _context.LotInfos.FindAsync(detail.LotId);
                        totalCostSaved += (lot?.PurchasePrice ?? 0) * detail.Quantity;
                    }
                }

                if (r.RefundType == "Adjustment")
                {
                    var customer = await _context.Customers.FindAsync(r.CustomerId);
                    decimal toAdvance = 0;
                    decimal fromDue = r.RefundAmount;
                    if (customer != null)
                    {
                        toAdvance = Math.Min(customer.AdvanceBalance, r.RefundAmount);
                        fromDue = r.RefundAmount - toAdvance;
                        if (toAdvance > 0) customer.AdvanceBalance -= toAdvance;
                        customer.CurrentDue += fromDue;
                        _context.Customers.Update(customer);
                    }
                    if (summary != null)
                    {
                        summary.TotalDueSales += fromDue;
                        summary.TotalCashSales += toAdvance;
                        _context.DailyTransactionSummaries.Update(summary);
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

                    if (summary != null)
                    {
                        summary.TotalCashSales += r.RefundAmount;
                        if (!isDigital) summary.TotalCash += r.RefundAmount;
                        _context.DailyTransactionSummaries.Update(summary);
                    }

                    // Remove the refund audit payment created for this return.
                    var refundPayment = await _context.InvoicePayments
                        .FirstOrDefaultAsync(p => p.TransactionNo == "REF-" + r.ReturnNo);
                    if (refundPayment != null)
                    {
                        _context.InvoicePayments.Remove(refundPayment);
                    }
                }
                else if (r.RefundType == "Credit" || r.RefundType == "Wallet")
                {
                    var customer = await _context.Customers.FindAsync(r.CustomerId);
                    if (customer != null)
                    {
                        if (customer.AdvanceBalance < r.RefundAmount)
                            throw new Exception($"Cannot delete return {r.ReturnNo}: the advance credit of {r.RefundAmount} has already been used.");
                        customer.AdvanceBalance -= r.RefundAmount;
                        _context.Customers.Update(customer);
                    }
                }

                // 3. Reverse the profit adjustment.
                if (summary != null)
                {
                    decimal profitLost = r.TotalAmount - totalCostSaved;
                    summary.NetProfit += profitLost;
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
                throw new Exception("Sales Return Delete Failed: " + ex.Message);
            }
        }

        public async Task<IEnumerable<SalesReturnMaster>> GetDeletedListAsync()
        {
            return await _context.SalesReturnMasters
                .IgnoreQueryFilters()
                .Include(r => r.Customer)
                .Include(r => r.Branch)
                .Where(r => r.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        // Restore re-applies the creation effects exactly as creation did
        // (and exactly undoes what SoftDeleteAsync reversed), in one
        // transaction.
        public async Task<bool> RestoreAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var r = await _context.SalesReturnMasters
                    .Include(x => x.ReturnDetails)
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.ReturnId == id);
                if (r == null) return false;
                if (!r.IsDeleted) return false;

                // Ordering guard (R3): a return cannot be restored while
                // its sale is deleted — the sale delete already unwound
                // the sale, so re-applying the return alone would
                // double-apply the refund. Restore the sale first. (A
                // return with no linked sale has no invoice to order
                // against, so the guard does not apply to it.)
                if (r.SalesId.HasValue)
                {
                    var sale = await _context.SalesMasters
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(x => x.SalesId == r.SalesId.Value);
                    if (sale != null && sale.IsDeleted)
                        throw new Exception($"Cannot restore sales return {r.ReturnNo}: the sale {sale.InvoiceNo} is deleted. Restore the sale first, then restore the return.");
                }

                decimal totalCostSaved = 0;
                foreach (var detail in r.ReturnDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null) continue;

                    if (!product.IsService)
                    {
                        var stock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == r.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);
                        if (stock == null)
                        {
                            stock = new BranchLotStock { BranchId = r.BranchId, ProductId = detail.ProductId, LotId = detail.LotId, CurrentStock = detail.Quantity };
                            await _context.BranchLotStocks.AddAsync(stock);
                        }
                        else
                        {
                            stock.CurrentStock += detail.Quantity;
                            _context.BranchLotStocks.Update(stock);
                        }

                        product.CurrentStock += detail.Quantity;
                        _context.Products.Update(product);

                        var lastLedger = await _context.StockLedgers
                            .Where(l => l.BranchId == r.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                            .OrderByDescending(l => l.LedgerId)
                            .FirstOrDefaultAsync();
                        await _context.StockLedgers.AddAsync(new StockLedger
                        {
                            BranchId = r.BranchId, ProductId = detail.ProductId, LotId = detail.LotId,
                            TransactionType = "SalesReturn Restore", ReferenceNo = r.ReturnNo, TransactionDate = DateTime.Now,
                            PreviousBalance = lastLedger?.CurrentBalance ?? 0, QuantityIn = detail.Quantity, QuantityOut = 0
                        });

                        var lot = await _context.LotInfos.FindAsync(detail.LotId);
                        totalCostSaved += (lot?.PurchasePrice ?? 0) * detail.Quantity;
                    }
                }

                var summary = await GetOrCreateSummaryAsync(r.ReturnDate.Date, r.BranchId);

                if (r.RefundType == "Adjustment")
                {
                    var customer = await _context.Customers.FindAsync(r.CustomerId);
                    decimal fromDue = 0;
                    decimal toAdvance = r.RefundAmount;
                    if (customer != null)
                    {
                        fromDue = Math.Min(customer.CurrentDue, r.RefundAmount);
                        toAdvance = r.RefundAmount - fromDue;
                        customer.CurrentDue -= fromDue;
                        if (toAdvance > 0) customer.AdvanceBalance += toAdvance;
                        _context.Customers.Update(customer);
                    }
                    summary.TotalDueSales -= fromDue;
                    if (summary.TotalDueSales < 0) summary.TotalDueSales = 0;
                    summary.TotalCashSales -= toAdvance;
                    if (summary.TotalCashSales < 0) summary.TotalCashSales = 0;
                }
                else if (r.RefundType == "Cash")
                {
                    bool isDigital = false;
                    if (r.PaymentMethodId.HasValue)
                    {
                        var method = await _context.PaymentMethods.FindAsync(r.PaymentMethodId.Value);
                        if (method != null && method.IsDigital) isDigital = true;
                    }

                    summary.TotalCashSales -= r.RefundAmount;
                    if (summary.TotalCashSales < 0) summary.TotalCashSales = 0;
                    if (!isDigital) summary.TotalCash -= r.RefundAmount;

                    // Re-create the refund audit payment (delete removed it);
                    // skip if one already exists for this return.
                    var existing = await _context.InvoicePayments.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.TransactionNo == "REF-" + r.ReturnNo);
                    if (existing == null)
                    {
                        await _context.InvoicePayments.AddAsync(new InvoicePayment
                        {
                            SaleId = r.SalesId,
                            PaymentDate = DateTime.Now,
                            Amount = -r.RefundAmount, // Negative to represent refund
                            PaymentMethodId = r.PaymentMethodId ?? 1,
                            TransactionType = "Refund",
                            TransactionNo = "REF-" + r.ReturnNo,
                            Remarks = "Refund for Return No: " + r.ReturnNo
                        });
                    }
                    else if (existing.IsDeleted)
                    {
                        existing.IsDeleted = false;
                        _context.InvoicePayments.Update(existing);
                    }
                }
                else if (r.RefundType == "Credit" || r.RefundType == "Wallet")
                {
                    var customer = await _context.Customers.FindAsync(r.CustomerId);
                    if (customer != null)
                    {
                        customer.AdvanceBalance += r.RefundAmount;
                        _context.Customers.Update(customer);
                    }
                }

                decimal profitLost = r.TotalAmount - totalCostSaved;
                summary.NetProfit -= profitLost;
                _context.DailyTransactionSummaries.Update(summary);

                r.IsDeleted = false;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Sales Return Restore Failed: " + ex.Message);
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
