using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Repo.Data;
using IPCS_Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class GlobalUnitConversionService : IGlobalUnitConversionService
    {
        private readonly IPCSDBContext _context; // আপনার DbContext এর নাম দিন

        public GlobalUnitConversionService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GlobalUnitConversionDTO>> GetAllAsync()
        {
            return await _context.GlobalUnitConversions
                .Include(g => g.FromUnit)
                .Include(g => g.ToUnit)
                .Where(g => g.IsActive)
                .Select(g => new GlobalUnitConversionDTO
                {
                    Id = g.Id,
                    FromUnitId = g.FromUnitId,
                    FromUnitName = g.FromUnit.UOMName,
                    ToUnitId = g.ToUnitId,
                    ToUnitName = g.ToUnit.UOMName,
                    ConversionFactor = g.ConversionFactor,
                    IsActive = g.IsActive
                }).ToListAsync();
        }

        public async Task<bool> CreateAsync(GlobalUnitConversion entity)
        {
            _context.GlobalUnitConversions.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var data = await _context.GlobalUnitConversions.FindAsync(id);
            if (data == null) return false;

            data.IsActive = false; // Soft delete
            _context.GlobalUnitConversions.Update(data);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
