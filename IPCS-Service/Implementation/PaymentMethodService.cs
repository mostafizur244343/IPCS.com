using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IPCSDBContext _context;

        public PaymentMethodService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentMethod>> GetAllAsync()
        {
            try { return await _context.PaymentMethods.ToListAsync(); }
            catch (Exception ex) { throw new Exception("Loading List Error " + ex.Message); }
        }

        public async Task<PaymentMethod?> GetByIdAsync(int id)
        {
            try { return await _context.PaymentMethods.FindAsync(id); }
            catch (Exception ex) { throw new Exception("Search error " + ex.Message); }
        }

        public async Task<bool> CreateAsync(PaymentMethod method)
        {
            try
            {
                await _context.PaymentMethods.AddAsync(method);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Save Error... " + ex.Message); }
        }

        public async Task<bool> UpdateAsync(PaymentMethod method)
        {
            try
            {
                _context.PaymentMethods.Update(method);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Update Error... " + ex.Message); }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var method = await _context.PaymentMethods.FindAsync(id);
                if (method == null) return false;

                // Ordering guard: a payment method used by live records
                // cannot be deleted — the DailySummary bucket math looks
                // the method up by id, so a deleted method would silently
                // skip buckets and the books would go off, and the payment
                // / purchase / return lists would lose the method lookup.
                // Delete the using records first.
                if (await _context.InvoicePayments.AnyAsync(p => p.PaymentMethodId == id))
                    throw new Exception($"Cannot delete payment method {method.MethodName}: it is used by one or more payments. Delete the payments first, then delete the payment method.");
                if (await _context.PurchaseMasters.AnyAsync(p => p.MethodId == id))
                    throw new Exception($"Cannot delete payment method {method.MethodName}: it is used by one or more purchases. Delete the purchases first, then delete the payment method.");
                if (await _context.PurchaseReturnMasters.AnyAsync(r => r.PaymentMethodId == id))
                    throw new Exception($"Cannot delete payment method {method.MethodName}: it is used by one or more purchase returns. Delete the returns first, then delete the payment method.");
                if (await _context.SalesReturnMasters.AnyAsync(r => r.PaymentMethodId == id))
                    throw new Exception($"Cannot delete payment method {method.MethodName}: it is used by one or more sales returns. Delete the returns first, then delete the payment method.");

                // Soft delete: the row stays in the DB (trash/restorable)
                // instead of being hard-removed.
                method.IsDeleted = true;
                _context.PaymentMethods.Update(method);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Delete Error... " + ex.Message); }
        }

        // Master data carries no books effects, so restore only flips the
        // flag back.
        public async Task<bool> RestoreAsync(int id)
        {
            var method = await _context.PaymentMethods.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.MethodId == id);
            if (method == null) return false;
            if (!method.IsDeleted) return false;
            method.IsDeleted = false;
            _context.PaymentMethods.Update(method);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<PaymentMethod>> GetDeletedListAsync()
        {
            return await _context.PaymentMethods.IgnoreQueryFilters()
                .Where(m => m.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}