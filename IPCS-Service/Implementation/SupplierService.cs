using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class SupplierService : ISupplierService
    {
        private readonly IPCSDBContext _context;
        public SupplierService(IPCSDBContext context) { _context = context; }

        public async Task<string> GenerateSupplierCodeAsync()
        {
            // AsNoTracking For getting Last ID
            var last = await _context.Suppliers.IgnoreQueryFilters().AsNoTracking().OrderByDescending(s => s.SupplierId).FirstOrDefaultAsync();
            int nextId = (last == null) ? 1 : last.SupplierId + 1;
            return $"SUP-{nextId:D4}";
        }

        public async Task<IEnumerable<Supplier>> GetAllActiveAsync() =>
            await _context.Suppliers.AsNoTracking().ToListAsync();

        public async Task<IEnumerable<Supplier>> GetDeletedListAsync() =>
            await _context.Suppliers.IgnoreQueryFilters().Where(s => s.IsDeleted).AsNoTracking().ToListAsync();

        public async Task<bool> CreateAsync(Supplier supplier)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                supplier.SupplierCode = await GenerateSupplierCodeAsync();
                // Openning Balance And Current Due Stay Same on Starting
                supplier.CurrentDue = supplier.OpeningBalance;

                await _context.Suppliers.AddAsync(supplier);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch { await transaction.RollbackAsync(); throw; }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var sup = await _context.Suppliers.FindAsync(id);
            if (sup == null) return false;
            sup.IsDeleted = true; // Just Changing The Flag
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            // For Geting Deleted Data IgnoreQueryFilters 
            var sup = await _context.Suppliers.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SupplierId == id);
            if (sup == null) return false;
            sup.IsDeleted = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Supplier?> GetByIdAsync(int id) => await _context.Suppliers.FindAsync(id);

        public async Task<bool> UpdateAsync(Supplier supplier)
        {
            _context.Entry(supplier).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}