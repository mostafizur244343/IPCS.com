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
    public class SalesService : ISalesService
    {
        private readonly IPCSDBContext _context;

        public SalesService(IPCSDBContext context)
        {
            _context = context;
        }

        private async Task<string> GenerateInvoiceNoAsync()
        {
            var last = await _context.SalesMasters.IgnoreQueryFilters().AsNoTracking().OrderByDescending(s => s.SalesId).FirstOrDefaultAsync();
            int nextId = (last == null) ? 1 : last.SalesId + 1;
            return $"INV-{nextId:D6}";
        }

        public async Task<IEnumerable<SalesMaster>> GetAllActiveAsync(string? search = null)
        {
            var query = _context.SalesMasters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(s => s.InvoiceNo.ToLower().Contains(term)
                    || (s.Customer != null && s.Customer.CustomerName.ToLower().Contains(term)));
            }

            return await query
                .Include(s => s.Customer)
                .Include(s => s.Branch)
                .OrderByDescending(s => s.SalesDate)
                .ToListAsync();
        }

        /// <summary>
        /// Server-side paging for the sales list. Projects to SalesListDTO in
        /// the database (no full entities, no Includes) and pages there, so a
        /// page costs one small query instead of the whole Sales table.
        /// </summary>
        public async Task<PagedResult<SalesListDTO>> GetPagedListAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.SalesMasters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(s => s.InvoiceNo.ToLower().Contains(term)
                    || (s.Customer != null && s.Customer.CustomerName.ToLower().Contains(term)));
            }

            var projected = query
                .OrderByDescending(s => s.SalesDate)
                .ThenByDescending(s => s.SalesId)
                .Select(s => new SalesListDTO
                {
                    SalesId = s.SalesId,
                    InvoiceNo = s.InvoiceNo,
                    SalesDate = s.SalesDate,
                    CustomerName = s.Customer != null ? s.Customer.CustomerName : "Walk-in Customer",
                    CustomerId = s.CustomerId,
                    TotalAmount = s.TotalAmount,
                    NetAmount = s.NetAmount,
                    PaidAmount = s.PaidAmount,
                    DueAmount = s.DueAmount,
                    PaymentStatus = s.PaymentStatus
                });

            return await projected.ToPagedResultAsync(page, pageSize);
        }

        /// <summary>
        /// Lightweight dashboard payload: today's sales total for the given
        /// day plus the 5 most recent sales. The day is passed as a [from, to)
        /// UTC range by the frontend (its local day), which reproduces exactly
        /// the old client-side "sales of today" filter without downloading the
        /// whole Sales table. With no range, the server's current day is used.
        /// </summary>
        public async Task<SalesDashboardSummaryDTO> GetDashboardSummaryAsync(DateTime? from, DateTime? to)
        {
            var start = from ?? DateTime.Today;
            var end = to ?? DateTime.Today.AddDays(1);

            var todayQuery = _context.SalesMasters
                .AsNoTracking()
                .Where(s => s.SalesDate >= start && s.SalesDate < end);

            var total = await todayQuery.SumAsync(s => (decimal?)s.NetAmount) ?? 0;
            var count = await todayQuery.CountAsync();

            var recent = await _context.SalesMasters
                .AsNoTracking()
                .OrderByDescending(s => s.SalesDate)
                .ThenByDescending(s => s.SalesId)
                .Take(5)
                .Select(s => new SalesListDTO
                {
                    SalesId = s.SalesId,
                    InvoiceNo = s.InvoiceNo,
                    SalesDate = s.SalesDate,
                    CustomerName = s.Customer != null ? s.Customer.CustomerName : "Walk-in Customer",
                    CustomerId = s.CustomerId,
                    TotalAmount = s.TotalAmount,
                    NetAmount = s.NetAmount,
                    PaidAmount = s.PaidAmount,
                    DueAmount = s.DueAmount,
                    PaymentStatus = s.PaymentStatus
                })
                .ToListAsync();

            return new SalesDashboardSummaryDTO
            {
                TodaySalesTotal = total,
                TodaySalesCount = count,
                RecentSales = recent
            };
        }

        public async Task<IEnumerable<SalesMaster>> GetDeletedListAsync()
        {
            return await _context.SalesMasters
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Branch)
                .Where(s => s.IsDeleted)
                .ToListAsync();
        }

        public async Task<SalesMaster?> GetByIdAsync(int id)
        {
            return await _context.SalesMasters
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Branch)
                .Include(s => s.SalesDetails)
                    .ThenInclude(d => d.Product)
                .Include(s => s.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .FirstOrDefaultAsync(s => s.SalesId == id);
        }

        public async Task<bool> CreateSalesAsync(SalesMaster salesMaster)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                salesMaster.InvoiceNo = await GenerateInvoiceNoAsync();
                decimal totalProfit = 0;

                // 0. Auto-calculate Paid, Due and Change Amount
                decimal totalPaidFromInput = salesMaster.Payments.Sum(p => p.Amount);
                if (totalPaidFromInput > salesMaster.NetAmount)
                {
                    salesMaster.ChangeAmount = totalPaidFromInput - salesMaster.NetAmount;
                    salesMaster.PaidAmount = salesMaster.NetAmount;
                    salesMaster.DueAmount = 0;
                    salesMaster.PaymentStatus = "Paid";

                    // Note: We don't subtract ChangeAmount from payments anymore 
                    // because we want to track the actual cash collected.
                }
                else
                {
                    salesMaster.ChangeAmount = 0;
                    salesMaster.PaidAmount = totalPaidFromInput;
                    salesMaster.DueAmount = salesMaster.NetAmount - totalPaidFromInput;
                    salesMaster.PaymentStatus = salesMaster.DueAmount <= 0 ? "Paid" : (salesMaster.PaidAmount > 0 ? "Partial" : "Due");
                }

                // 1. Process Stock & Details
                foreach (var detail in salesMaster.SalesDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null)
                    {
                        throw new Exception($"Product with ID {detail.ProductId} not found.");
                    }

                    // Skip stock logic for Services
                    if (!product.IsService)
                    {
                        // Validate Stock in Branch/Lot
                        var branchStock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == salesMaster.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);

                        if (branchStock == null)
                        {
                            throw new Exception($"No stock information found for {product.ProductName} in this branch/lot.");
                        }

                        if (branchStock.CurrentStock < detail.Quantity)
                        {
                            throw new Exception($"Insufficient stock for {product.ProductName}. Available: {branchStock.CurrentStock}");
                        }

                        // Deduct Stock
                        branchStock.CurrentStock -= detail.Quantity;
                        _context.BranchLotStocks.Update(branchStock);

                        // Update Global Product Stock
                        product.CurrentStock -= detail.Quantity;
                        _context.Products.Update(product);

                        // Stock Ledger Entry
                        var lastLedger = await _context.StockLedgers
                            .AsNoTracking()
                            .Where(l => l.BranchId == salesMaster.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                            .OrderByDescending(l => l.LedgerId)
                            .FirstOrDefaultAsync();

                        var ledger = new StockLedger
                        {
                            BranchId = salesMaster.BranchId,
                            ProductId = detail.ProductId,
                            LotId = detail.LotId,
                            TransactionType = "Sale",
                            ReferenceNo = salesMaster.InvoiceNo,
                            TransactionDate = DateTime.Now,
                            PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                            QuantityIn = 0,
                            QuantityOut = detail.Quantity
                        };
                        await _context.StockLedgers.AddAsync(ledger);
                    }

                    // Validate UOMId
                    if (detail.UOMId <= 0)
                    {
                         // Use Product's BaseUOM as fallback if not provided
                         detail.UOMId = product.UOMId; 
                    }
                    
                    var uom = await _context.UOMs.FindAsync(detail.UOMId);
                    if (uom == null)
                    {
                         throw new Exception($"Unit of Measure (UOM) not found for {product.ProductName}. Please check product setup.");
                    }

                    // Get Cost Price from Lot for Profit Calculation
                    var lot = await _context.LotInfos.AsNoTracking().FirstOrDefaultAsync(l => l.LotId == detail.LotId);
                    if (lot == null && !product.IsService)
                    {
                         throw new Exception($"Lot information not found for {product.ProductName}. Please select a valid lot.");
                    }
                    detail.CostPriceAtSale = lot?.PurchasePrice ?? 0;
                    
                    // Simple Profit: Revenue - (Cost * Qty)
                    totalProfit += (detail.LineTotal - (detail.CostPriceAtSale * detail.Quantity));
                }

                // 2. Update Customer Due or Advance Credit
                var customer = await _context.Customers.FindAsync(salesMaster.CustomerId);
                if (customer != null)
                {
                    if (salesMaster.DueAmount > 0)
                    {
                        customer.CurrentDue += salesMaster.DueAmount;
                    }

                    if (salesMaster.ChangeAmount > 0 && salesMaster.IsChangeConvertedToCredit)
                    {
                        customer.AdvanceBalance += salesMaster.ChangeAmount;
                    }
                    
                    _context.Customers.Update(customer);
                }

                // 3. Update Daily Transaction Summary
                var today = DateTime.Now.Date;
                var summary = await _context.DailyTransactionSummaries.FirstOrDefaultAsync(d => d.TransactionDate == today && d.BranchId == salesMaster.BranchId);
                if (summary == null)
                {
                    summary = new DailyTransactionSummary 
                    { 
                        TransactionDate = today, 
                        BranchId = salesMaster.BranchId,
                        OpeningBalance = 0 // Should ideally fetch from yesterday
                    };
                    await _context.DailyTransactionSummaries.AddAsync(summary);
                }

                summary.TotalCashSales += salesMaster.PaidAmount; 
                
                // If Change is taken as income, add it to Cash Sales and Profit
                if (salesMaster.ChangeAmount > 0 && salesMaster.IsChangeTakenAsIncome)
                {
                    summary.TotalCashSales += salesMaster.ChangeAmount;
                    totalProfit += salesMaster.ChangeAmount; // Extra income is pure profit
                }

                summary.TotalDueSales += salesMaster.DueAmount;
                summary.NetProfit += totalProfit;

                // Update specific method buckets
                foreach(var payment in salesMaster.Payments)
                {
                    var method = await _context.PaymentMethods.AsNoTracking().FirstOrDefaultAsync(m => m.MethodId == payment.PaymentMethodId);
                    if (method != null)
                    {
                        string name = method.MethodName.ToLower();
                        if (name.Contains("cash")) summary.TotalCash += payment.Amount;
                        else if (name.Contains("bkash")) summary.TotalBkash += payment.Amount;
                        else if (name.Contains("nagad")) summary.TotalNagad += payment.Amount;
                        else if (name.Contains("card") || name.Contains("bank")) summary.TotalCard += payment.Amount;
                    }
                }

                // 4. Final Save
                await _context.SalesMasters.AddAsync(salesMaster);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Sales Error Trace: " + ex.ToString());
            }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = await _context.SalesMasters
                    .Include(s => s.SalesDetails)
                    .Include(s => s.Payments)
                    .FirstOrDefaultAsync(s => s.SalesId == id);
                if (sale == null) return false;

                // (Ordering guard R4 was removed 2026-11-26: a sale whose
                // return is already deleted CAN be deleted. The return's
                // refund audit payment (REF-) was already hard-removed
                // by the return's own delete, so the delete and restore
                // bucket loops both consistently never see it. R1 (a
                // return cannot be deleted while its sale is deleted)
                // guarantees no return can change state between this
                // sale's delete and restore, so the payment set is
                // identical at both ends and the books reconcile
                // exactly. See GUARD_RELAX.md.)

                // Guard (Finding 1, 2026-11-26): a sale with a live
                // return cannot be deleted. Deleting it reverses
                // TotalCashSales by the full paid amount, but a live
                // return has already taken its refund out of
                // TotalCashSales; the >= 0 clamp then swallows the
                // difference and restore adds the full amount back,
                // stranding the refund amount in the day's summary
                // (verified live: +2.40). Delete the return first —
                // its own delete reverses the refund exactly — then
                // delete the sale. This mirrors the purchase side,
                // where the stock guard forces the same return-first
                // order. Only live returns block: a return deleted
                // before this sale's delete is already gone (its REF-
                // payment with it), so delete/restore see the same
                // payment set and reconcile exactly, and the old
                // R2/R4 ordering deadlock cannot come back — the
                // working order stays delete-return-first, then
                // invoice; restore-invoice-first, then return.
                var liveReturn = await _context.SalesReturnMasters
                    .FirstOrDefaultAsync(x => x.SalesId == id && !x.IsDeleted);
                if (liveReturn != null)
                    throw new Exception($"Cannot delete sale {sale.InvoiceNo}: sales return {liveReturn.ReturnNo} is still active. Delete the return first, then delete the sale.");

                // 1. Reverse stock: add the sold quantities back to their lots.
                // Guard: the lot/branch-stock row must still exist, and a
                // converted change credit must still be held by the customer.
                var reversals = new List<(SalesDetails Detail, BranchLotStock Stock, Product Product)>();
                foreach (var detail in sale.SalesDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null && !product.IsService)
                    {
                        var stock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == sale.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);
                        if (stock == null)
                            throw new Exception($"Cannot delete sale {sale.InvoiceNo}: stock record not found for Product ID {detail.ProductId} (lot {detail.LotId}).");
                        reversals.Add((detail, stock, product));
                    }
                }

                if (sale.ChangeAmount > 0 && sale.IsChangeConvertedToCredit)
                {
                    var advanceCustomer = await _context.Customers.FindAsync(sale.CustomerId);
                    if (advanceCustomer != null && advanceCustomer.AdvanceBalance < sale.ChangeAmount)
                        throw new Exception($"Cannot delete sale {sale.InvoiceNo}: the change credit of {sale.ChangeAmount} has already been used.");
                }

                foreach (var (detail, stock, product) in reversals)
                {
                    stock.CurrentStock += detail.Quantity;
                    _context.BranchLotStocks.Update(stock);

                    product.CurrentStock += detail.Quantity;
                    _context.Products.Update(product);

                    var lastLedger = await _context.StockLedgers.AsNoTracking()
                        .Where(l => l.BranchId == sale.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                        .OrderByDescending(l => l.LedgerId)
                        .FirstOrDefaultAsync();
                    await _context.StockLedgers.AddAsync(new StockLedger
                    {
                        BranchId = sale.BranchId,
                        ProductId = detail.ProductId,
                        LotId = detail.LotId,
                        TransactionType = "Sale Delete",
                        ReferenceNo = sale.InvoiceNo,
                        TransactionDate = DateTime.Now,
                        PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                        QuantityIn = detail.Quantity,
                        QuantityOut = 0
                    });
                }

                // 2. Reverse customer due / advance credit.
                var customer = await _context.Customers.FindAsync(sale.CustomerId);
                if (customer != null)
                {
                    if (sale.DueAmount > 0) customer.CurrentDue -= sale.DueAmount;
                    if (sale.ChangeAmount > 0 && sale.IsChangeConvertedToCredit) customer.AdvanceBalance -= sale.ChangeAmount;
                    _context.Customers.Update(customer);
                }

                // 3. Reverse the DailySummary effects (mirror of CreateSalesAsync).
                var summary = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(d => d.TransactionDate == sale.SalesDate.Date && d.BranchId == sale.BranchId);
                if (summary != null)
                {
                    // Split settlements never went into TotalCashSales at
                    // creation — they went to TotalCollection — so the
                    // delete reverses TotalCashSales by PaidAmount minus
                    // the live split settlements, and TotalCollection by
                    // the live split settlements. (Previously it subtracted
                    // the full current PaidAmount, which overstated the
                    // reversal by the split amount and, through the < 0
                    // clamp, could wipe out other sales' contributions;
                    // and TotalCollection was never reversed at all, so a
                    // settled due sale left it stranded after delete.)
                    decimal liveSplitTotal = (sale.Payments ?? Enumerable.Empty<InvoicePayment>())
                        .Where(p => p.IsSplitSettlement)
                        .Sum(p => p.Amount);

                    summary.TotalCashSales -= (sale.PaidAmount - liveSplitTotal);
                    if (sale.ChangeAmount > 0 && sale.IsChangeTakenAsIncome) summary.TotalCashSales -= sale.ChangeAmount;
                    if (summary.TotalCashSales < 0) summary.TotalCashSales = 0;

                    summary.TotalCollection -= liveSplitTotal;
                    if (summary.TotalCollection < 0) summary.TotalCollection = 0;

                    summary.TotalDueSales -= sale.DueAmount;
                    if (summary.TotalDueSales < 0) summary.TotalDueSales = 0;

                    // Recompute the profit this sale added, exactly as creation did.
                    decimal totalProfit = sale.SalesDetails.Sum(d => d.LineTotal - (d.CostPriceAtSale * d.Quantity));
                    if (sale.ChangeAmount > 0 && sale.IsChangeTakenAsIncome) totalProfit += sale.ChangeAmount;
                    summary.NetProfit -= totalProfit;

                    // The sale's payments are unwound together with the voided
                    // sale; the payment rows themselves are kept as records.
                    foreach (var payment in sale.Payments ?? Enumerable.Empty<InvoicePayment>())
                    {
                        var method = await _context.PaymentMethods.AsNoTracking().FirstOrDefaultAsync(m => m.MethodId == payment.PaymentMethodId);
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
                }

                sale.IsDeleted = true;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Sale Delete Failed: " + ex.Message, ex);
            }
        }

        // Header-only edit: Remarks and SalesDate only. Quantities, amounts,
        // customer, branch and lines are never touched, so stock and books
        // cannot be corrupted by this update.
        public async Task<bool> UpdateHeaderAsync(int id, DocumentHeaderUpdateDTO dto)
        {
            var sale = await _context.SalesMasters.FindAsync(id);
            if (sale == null) return false;

            if (dto.Remarks != null) sale.Remarks = dto.Remarks;
            if (dto.DocumentDate.HasValue) sale.SalesDate = dto.DocumentDate.Value;
            return await _context.SaveChangesAsync() > 0;
        }

        // Restore re-applies the creation effects exactly as creation did
        // (and exactly undoes what SoftDeleteAsync reversed), in one
        // transaction. Guard: if the sold goods are no longer in stock
        // (sold on or returned after the sale was deleted), the restore is
        // rejected with a clear error instead of driving stock negative.
        // Only payments that are still live are counted — a payment
        // soft-deleted earlier already had its bucket effect reversed by
        // its own delete, so it must not be counted here.
        public async Task<bool> RestoreAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = await _context.SalesMasters
                    .Include(s => s.SalesDetails)
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.SalesId == id);
                if (sale == null) return false;
                if (!sale.IsDeleted) return false;

                // (Ordering guard R2 was removed 2026-11-26, for the same
                // reason as R4 above: a return deleted before this
                // sale's delete is already gone — its REF- payment with
                // it — so restore re-applies exactly the payment set
                // delete unwound. R1 + R3 freeze every return's state
                // while the sale is deleted. See GUARD_RELAX.md.)

                // 1. Re-apply stock: remove the sold quantities again.
                var reversals = new List<(SalesDetails Detail, BranchLotStock Stock, Product Product)>();
                foreach (var detail in sale.SalesDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null && !product.IsService)
                    {
                        var stock = await _context.BranchLotStocks
                            .FirstOrDefaultAsync(s => s.BranchId == sale.BranchId && s.ProductId == detail.ProductId && s.LotId == detail.LotId);
                        if (stock == null)
                            throw new Exception($"Cannot restore sale {sale.InvoiceNo}: stock record not found for Product ID {detail.ProductId} (lot {detail.LotId}).");
                        if (stock.CurrentStock < detail.Quantity)
                            throw new Exception($"Cannot restore sale {sale.InvoiceNo}: only {stock.CurrentStock} units of {product.ProductName} remain in stock (needs {detail.Quantity}). The goods were sold or returned after the sale was deleted.");
                        reversals.Add((detail, stock, product));
                    }
                }

                foreach (var (detail, stock, product) in reversals)
                {
                    stock.CurrentStock -= detail.Quantity;
                    _context.BranchLotStocks.Update(stock);

                    product.CurrentStock -= detail.Quantity;
                    _context.Products.Update(product);

                    var lastLedger = await _context.StockLedgers.AsNoTracking()
                        .Where(l => l.BranchId == sale.BranchId && l.ProductId == detail.ProductId && l.LotId == detail.LotId)
                        .OrderByDescending(l => l.LedgerId)
                        .FirstOrDefaultAsync();
                    await _context.StockLedgers.AddAsync(new StockLedger
                    {
                        BranchId = sale.BranchId,
                        ProductId = detail.ProductId,
                        LotId = detail.LotId,
                        TransactionType = "Sale Restore",
                        ReferenceNo = sale.InvoiceNo,
                        TransactionDate = DateTime.Now,
                        PreviousBalance = lastLedger?.CurrentBalance ?? 0,
                        QuantityIn = 0,
                        QuantityOut = detail.Quantity
                    });
                }

                // 2. Re-apply customer due / advance credit.
                var customer = await _context.Customers.FindAsync(sale.CustomerId);
                if (customer != null)
                {
                    if (sale.DueAmount > 0) customer.CurrentDue += sale.DueAmount;
                    if (sale.ChangeAmount > 0 && sale.IsChangeConvertedToCredit) customer.AdvanceBalance += sale.ChangeAmount;
                    _context.Customers.Update(customer);
                }

                // 3. Re-apply the DailySummary effects (mirror of delete; the
                // summary row is the one for the sale date + branch, created
                // here if it is missing).
                var summary = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(d => d.TransactionDate == sale.SalesDate.Date && d.BranchId == sale.BranchId);
                if (summary == null)
                {
                    summary = new DailyTransactionSummary
                    {
                        TransactionDate = sale.SalesDate.Date,
                        BranchId = sale.BranchId,
                        OpeningBalance = 0
                    };
                    await _context.DailyTransactionSummaries.AddAsync(summary);
                }

                // The sale's live payments. Only the split settlements went
                // to TotalCollection at settlement time, so restore
                // re-applies TotalCashSales by PaidAmount minus the live
                // splits, and TotalCollection by the live splits — the
                // exact mirror of SoftDeleteAsync. (Previously restore
                // added the full current PaidAmount to TotalCashSales,
                // overstating it by the split amount, and never touched
                // TotalCollection.)
                var payments = await _context.InvoicePayments
                    .Where(p => p.SaleId == id)
                    .ToListAsync();
                decimal liveSplitTotal = payments.Where(p => p.IsSplitSettlement).Sum(p => p.Amount);

                summary.TotalCashSales += (sale.PaidAmount - liveSplitTotal);
                if (sale.ChangeAmount > 0 && sale.IsChangeTakenAsIncome) summary.TotalCashSales += sale.ChangeAmount;
                summary.TotalDueSales += sale.DueAmount;
                summary.TotalCollection += liveSplitTotal;

                // Recompute the profit this sale adds, exactly as creation did.
                decimal totalProfit = sale.SalesDetails.Sum(d => d.LineTotal - (d.CostPriceAtSale * d.Quantity));
                if (sale.ChangeAmount > 0 && sale.IsChangeTakenAsIncome) totalProfit += sale.ChangeAmount;
                summary.NetProfit += totalProfit;

                foreach (var payment in payments)
                {
                    var method = await _context.PaymentMethods.AsNoTracking().FirstOrDefaultAsync(m => m.MethodId == payment.PaymentMethodId);
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

                sale.IsDeleted = false;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Sale Restore Failed: " + ex.Message, ex);
            }
        }

    }
}
