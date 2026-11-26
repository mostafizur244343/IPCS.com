using IPCS_Model.Entities;
using IPCS_Repo.Data;
using IPCS_Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class ProductUnitConversionService : IProductUnitConversionService
    {
        private readonly IPCSDBContext _context;
        public ProductUnitConversionService(IPCSDBContext context) { _context = context; }

        public async Task<IEnumerable<ProductUnitConversion>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductUnitConversions
                .Include(c => c.FromUnit)
                .Include(c => c.ToUnit)
                .Where(c => c.ProductId == productId)
                .OrderBy(c => c.Level)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductUnitConversion>> GetAllConversionsAsync()
        {
            return await _context.ProductUnitConversions
                .Include(c => c.Product)
                .Include(c => c.FromUnit)
                .Include(c => c.ToUnit)
                .OrderBy(c => c.Level)
                .ToListAsync();
        }

        public async Task<ProductUnitConversion?> GetByIdAsync(int id)
        {
            return await _context.ProductUnitConversions
                .Include(c => c.Product)
                .Include(c => c.FromUnit)
                .Include(c => c.ToUnit)
                .FirstOrDefaultAsync(c => c.ConversionId == id);
        }

        public async Task<bool> AddConversionAsync(ProductUnitConversion conversion)
        {
            await _context.ProductUnitConversions.AddAsync(conversion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(ProductUnitConversion conversion)
        {
            var existing = await _context.ProductUnitConversions.FindAsync(conversion.ConversionId);
            if (existing == null) return false;

            existing.ProductId = conversion.ProductId;
            existing.FromUnitId = conversion.FromUnitId;
            existing.ToUnitId = conversion.ToUnitId;
            existing.Factor = conversion.Factor;
            existing.Level = conversion.Level;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteConversionAsync(int id)
        {
            var data = await _context.ProductUnitConversions.FindAsync(id);
            if (data == null) return false;
            // Soft delete: the row stays in the DB (trash/restorable)
            // instead of being hard-removed.
            data.IsDeleted = true;
            _context.ProductUnitConversions.Update(data);
            return await _context.SaveChangesAsync() > 0;
        }

        // Conversions carry no books effects, so restore only flips the
        // flag back.
        public async Task<bool> RestoreConversionAsync(int id)
        {
            var data = await _context.ProductUnitConversions.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.ConversionId == id);
            if (data == null) return false;
            if (!data.IsDeleted) return false;
            data.IsDeleted = false;
            _context.ProductUnitConversions.Update(data);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<ProductUnitConversion>> GetDeletedConversionsAsync()
        {
            return await _context.ProductUnitConversions.IgnoreQueryFilters()
                .Include(c => c.Product)
                .Include(c => c.FromUnit)
                .Include(c => c.ToUnit)
                .Where(c => c.IsDeleted)
                .OrderBy(c => c.Level)
                .ToListAsync();
        }
    }
}