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
    public class PaymentService : IPaymentService
    {
        private readonly IPCSDBContext _context;

        public PaymentService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InvoicePayment>> GetPaymentsByInvoiceAsync(string type, int invoiceId)
        {
            return await _context.InvoicePayments
                .Include(p => p.PaymentMethod)
                .Where(p => p.TransactionType == type && (p.SaleId == invoiceId || p.PurchaseId == invoiceId))
                .ToListAsync();
        }

        public async Task<bool> ProcessSplitPaymentAsync(List<InvoicePaymentDTO> payments)
        {
            if (payments == null || !payments.Any()) return false;

            foreach (var dto in payments)
            {
                var payment = new InvoicePayment
                {
                    TransactionType = dto.TransactionType,
                    SaleId = dto.SaleId,
                    PurchaseId = dto.PurchaseId,
                    PaymentMethodId = dto.PaymentMethodId,
                    Amount = dto.Amount,
                    PaymentDate = dto.PaymentDate,
                    TransactionNo = dto.TransactionNo,
                    Remarks = dto.Remarks
                };

                await _context.InvoicePayments.AddAsync(payment);

                // Settle the invoice and the supplier/customer due balance.
                // (Previously the payment row was inserted but the invoice was
                // never updated, so Paid/Due/Status and the party due stayed stale.)
                int? branchId = null;
                if ((dto.TransactionType == "Purchase" || dto.TransactionType == "SupplierPayment") && dto.PurchaseId.HasValue)
                {
                    var purchase = await _context.PurchaseMasters.FindAsync(dto.PurchaseId.Value);
                    if (purchase != null)
                    {
                        branchId = purchase.BranchId;
                        purchase.PaidAmount += dto.Amount;
                        purchase.DueAmount -= dto.Amount;
                        purchase.PaymentStatus = purchase.DueAmount <= 0 ? "Paid" : "Partial";
                        _context.PurchaseMasters.Update(purchase);

                        var supplier = await _context.Suppliers.FindAsync(purchase.SupplierId);
                        if (supplier != null)
                        {
                            supplier.CurrentDue -= dto.Amount;
                            _context.Suppliers.Update(supplier);
                        }
                    }
                }
                else if ((dto.TransactionType == "Sale" || dto.TransactionType == "CustomerReceive") && dto.SaleId.HasValue)
                {
                    var sale = await _context.SalesMasters.FindAsync(dto.SaleId.Value);
                    if (sale != null)
                    {
                        branchId = sale.BranchId;
                        sale.PaidAmount += dto.Amount;
                        sale.DueAmount -= dto.Amount;
                        sale.PaymentStatus = sale.DueAmount <= 0 ? "Paid" : "Partial";
                        _context.SalesMasters.Update(sale);

                        var customer = await _context.Customers.FindAsync(sale.CustomerId);
                        if (customer != null)
                        {
                            customer.CurrentDue -= dto.Amount;
                            _context.Customers.Update(customer);
                        }
                    }
                }

                // Update Daily Transaction Summary based on Method
                await UpdateDailySummaryAsync(dto.PaymentMethodId, dto.Amount, dto.TransactionType, branchId);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        private async Task UpdateDailySummaryAsync(int methodId, decimal amount, string type, int? branchId = null)
        {
            var today = DateTime.Now.Date;
            var summary = branchId.HasValue
                ? await _context.DailyTransactionSummaries.FirstOrDefaultAsync(d => d.TransactionDate == today && d.BranchId == branchId.Value)
                : await _context.DailyTransactionSummaries.FirstOrDefaultAsync(d => d.TransactionDate == today);
            
            if (summary == null)
            {
                summary = new DailyTransactionSummary { TransactionDate = today, BranchId = branchId ?? 0 };
                await _context.DailyTransactionSummaries.AddAsync(summary);
            }

            // Logic: If Sale, it's a Collection (+). If Purchase, it's a Payment (-)
            if (type == "Sale" || type == "CustomerReceive")
            {
                summary.TotalCollection += amount;
                await AddToMethodBucketAsync(summary, methodId, amount);
            }
            else if (type == "Purchase" || type == "SupplierPayment")
            {
                // Cash outflow: reduce the payment-method bucket. (Previously
                // this branch did nothing, so split supplier payments never
                // showed up in any cash/bKash/Nagad/Card bucket.)
                await AddToMethodBucketAsync(summary, methodId, -amount);
            }
        }

        private async Task AddToMethodBucketAsync(DailyTransactionSummary summary, int methodId, decimal amount)
        {
            var method = await _context.PaymentMethods.FindAsync(methodId);
            if (method == null) return;
            string name = method.MethodName.ToLower();
            if (name.Contains("cash")) summary.TotalCash += amount;
            else if (name.Contains("bkash")) summary.TotalBkash += amount;
            else if (name.Contains("nagad")) summary.TotalNagad += amount;
            else if (name.Contains("card") || name.Contains("bank")) summary.TotalCard += amount;
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var payment = await _context.InvoicePayments.FindAsync(paymentId);
            if (payment == null) return false;

            // Reverse the invoice settlement this payment made (mirror of
            // ProcessSplitPaymentAsync). If the linked invoice is soft-deleted
            // or missing, its effects were already unwound (or never existed),
            // so only the payment row itself is removed.
            int? branchId = null;
            bool invoiceFound = false;
            if ((payment.TransactionType == "Purchase" || payment.TransactionType == "SupplierPayment") && payment.PurchaseId.HasValue)
            {
                var purchase = await _context.PurchaseMasters.FindAsync(payment.PurchaseId.Value);
                if (purchase != null)
                {
                    invoiceFound = true;
                    branchId = purchase.BranchId;
                    purchase.PaidAmount -= payment.Amount;
                    purchase.DueAmount += payment.Amount;
                    purchase.PaymentStatus = purchase.DueAmount <= 0 ? "Paid" : (purchase.PaidAmount > 0 ? "Partial" : "Due");
                    _context.PurchaseMasters.Update(purchase);

                    var supplier = await _context.Suppliers.FindAsync(purchase.SupplierId);
                    if (supplier != null)
                    {
                        supplier.CurrentDue += payment.Amount;
                        _context.Suppliers.Update(supplier);
                    }
                }
            }
            else if ((payment.TransactionType == "Sale" || payment.TransactionType == "CustomerReceive") && payment.SaleId.HasValue)
            {
                var sale = await _context.SalesMasters.FindAsync(payment.SaleId.Value);
                if (sale != null)
                {
                    invoiceFound = true;
                    branchId = sale.BranchId;
                    sale.PaidAmount -= payment.Amount;
                    sale.DueAmount += payment.Amount;
                    sale.PaymentStatus = sale.DueAmount <= 0 ? "Paid" : (sale.PaidAmount > 0 ? "Partial" : "Due");
                    _context.SalesMasters.Update(sale);

                    var customer = await _context.Customers.FindAsync(sale.CustomerId);
                    if (customer != null)
                    {
                        customer.CurrentDue += payment.Amount;
                        _context.Customers.Update(customer);
                    }
                }
            }

            // Reverse the DailySummary buckets, but only when the payment had
            // a live invoice (its bucket effect was actually recorded).
            if (invoiceFound)
            {
                await UpdateDailySummaryAsync(payment.PaymentMethodId, -payment.Amount, payment.TransactionType, branchId);
            }

            // Soft delete: the row stays as a record (visible in trash and
            // restorable) instead of being hard-removed.
            payment.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<InvoicePayment>> GetDeletedPaymentsAsync()
        {
            return await _context.InvoicePayments
                .IgnoreQueryFilters()
                .Include(p => p.PaymentMethod)
                .Where(p => p.IsDeleted)
                .ToListAsync();
        }

        // Restore re-applies the settlement exactly as ProcessSplitPaymentAsync
        // did (and exactly undoes what DeletePaymentAsync reversed). Guard:
        // the linked invoice must be live — if the invoice itself is deleted,
        // its settlement was already unwound with it, so restoring the
        // payment alone would double-apply. Restore the invoice first.
        public async Task<bool> RestorePaymentAsync(int paymentId)
        {
            var payment = await _context.InvoicePayments.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null) return false;
            if (!payment.IsDeleted) return false;

            int? branchId = null;
            bool invoiceFound = false;
            if ((payment.TransactionType == "Purchase" || payment.TransactionType == "SupplierPayment") && payment.PurchaseId.HasValue)
            {
                // Guard (R5): the linked purchase must be live — if the
                // purchase itself is deleted, its settlement was already
                // unwound with it, so restoring the payment alone would
                // double-apply. Restore the purchase first.
                var purchase = await _context.PurchaseMasters.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.PurchaseId == payment.PurchaseId.Value);
                if (purchase == null)
                    throw new Exception($"Cannot restore payment #{payment.PaymentId}: the purchase linked to this payment no longer exists.");
                if (purchase.IsDeleted)
                    throw new Exception($"Cannot restore payment #{payment.PaymentId}: the purchase {purchase.PurchaseCode} is deleted. Restore the purchase first, then restore the payment.");
                invoiceFound = true;
                branchId = purchase.BranchId;
                purchase.PaidAmount += payment.Amount;
                purchase.DueAmount -= payment.Amount;
                purchase.PaymentStatus = purchase.DueAmount <= 0 ? "Paid" : (purchase.PaidAmount > 0 ? "Partial" : "Due");
                _context.PurchaseMasters.Update(purchase);

                var supplier = await _context.Suppliers.FindAsync(purchase.SupplierId);
                if (supplier != null)
                {
                    supplier.CurrentDue -= payment.Amount;
                    _context.Suppliers.Update(supplier);
                }
            }
            else if ((payment.TransactionType == "Sale" || payment.TransactionType == "CustomerReceive") && payment.SaleId.HasValue)
            {
                // Guard (R5): the linked sale must be live — if the sale
                // itself is deleted, its settlement was already unwound
                // with it, so restoring the payment alone would
                // double-apply. Restore the sale first.
                var sale = await _context.SalesMasters.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.SalesId == payment.SaleId.Value);
                if (sale == null)
                    throw new Exception($"Cannot restore payment #{payment.PaymentId}: the sale linked to this payment no longer exists.");
                if (sale.IsDeleted)
                    throw new Exception($"Cannot restore payment #{payment.PaymentId}: the sale {sale.InvoiceNo} is deleted. Restore the sale first, then restore the payment.");
                invoiceFound = true;
                branchId = sale.BranchId;
                sale.PaidAmount += payment.Amount;
                sale.DueAmount -= payment.Amount;
                sale.PaymentStatus = sale.DueAmount <= 0 ? "Paid" : (sale.PaidAmount > 0 ? "Partial" : "Due");
                _context.SalesMasters.Update(sale);

                var customer = await _context.Customers.FindAsync(sale.CustomerId);
                if (customer != null)
                {
                    customer.CurrentDue -= payment.Amount;
                    _context.Customers.Update(customer);
                }
            }
            // Refund / audit payments (PurchaseReturnRefund, Refund) have no
            // invoice settlement to re-apply; only the row itself is restored.

            if (invoiceFound)
            {
                await UpdateDailySummaryAsync(payment.PaymentMethodId, payment.Amount, payment.TransactionType, branchId);
            }

            payment.IsDeleted = false;
            return await _context.SaveChangesAsync() > 0;
        }

        // Header-only edit: Remarks and TransactionNo only. Amount, payment
        // method and invoice links are never touched, so settlements and
        // DailySummary cannot be corrupted by this update.
        public async Task<bool> UpdateHeaderAsync(int paymentId, PaymentHeaderUpdateDTO dto)
        {
            var payment = await _context.InvoicePayments.FindAsync(paymentId);
            if (payment == null) return false;

            if (dto.Remarks != null) payment.Remarks = dto.Remarks;
            if (dto.TransactionNo != null) payment.TransactionNo = dto.TransactionNo;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<decimal> GetTotalCollectionByMethodAsync(int methodId, DateTime? date = null)
        {
            var query = _context.InvoicePayments.Where(p => p.PaymentMethodId == methodId);
            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(p => p.PaymentDate.Date == targetDate);
            }

            return await query.SumAsync(p => p.Amount);
        }
    }
}
