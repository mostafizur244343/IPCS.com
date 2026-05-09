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

                // Update Daily Transaction Summary based on Method
                await UpdateDailySummaryAsync(dto.PaymentMethodId, dto.Amount, dto.TransactionType);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        private async Task UpdateDailySummaryAsync(int methodId, decimal amount, string type)
        {
            var today = DateTime.Now.Date;
            var summary = await _context.DailyTransactionSummaries.FirstOrDefaultAsync(d => d.TransactionDate == today);
            
            if (summary == null)
            {
                summary = new DailyTransactionSummary { TransactionDate = today };
                await _context.DailyTransactionSummaries.AddAsync(summary);
            }

            // Logic: If Sale, it's a Collection (+). If Purchase, it's a Payment (-)
            if (type == "Sale" || type == "CustomerReceive")
            {
                summary.TotalCollection += amount;
                // You can also add method specific columns to DailySummary if you have them
            }
            else if (type == "Purchase" || type == "SupplierPayment")
            {
                // In daily summary, we track total purchase amount. 
                // Actual cash outflow can be tracked separately if needed.
            }
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var payment = await _context.InvoicePayments.FindAsync(paymentId);
            if (payment == null) return false;

            // Reverse summary if needed
            await UpdateDailySummaryAsync(payment.PaymentMethodId, -payment.Amount, payment.TransactionType);

            _context.InvoicePayments.Remove(payment);
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
