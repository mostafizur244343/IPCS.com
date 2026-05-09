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

        public async Task<bool> AddConversionAsync(ProductUnitConversion conversion)
        {
            await _context.ProductUnitConversions.AddAsync(conversion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteConversionAsync(int id)
        {
            var data = await _context.ProductUnitConversions.FindAsync(id);
            if (data == null) return false;
            _context.ProductUnitConversions.Remove(data);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}