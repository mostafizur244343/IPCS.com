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
                _context.PaymentMethods.Remove(method);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Delete Error... " + ex.Message); }
        }
    }
}