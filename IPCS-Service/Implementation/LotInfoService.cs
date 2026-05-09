
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class LotInfoService : ILotInfoService
    {
        private readonly IPCSDBContext _context;

        public LotInfoService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LotInfo>> GetAllAsync()
        {
            return await _context.LotInfos.Include(l => l.Product).ToListAsync();
        }

        public async Task<LotInfo?> GetByIdAsync(int id)
        {
            return await _context.LotInfos.Include(l => l.Product).FirstOrDefaultAsync(l => l.LotId == id);
        }

        public async Task<bool> CreateAsync(LotInfo lotInfo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.LotInfos.AddAsync(lotInfo);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // If All Process successfull then save 
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(); // If Error get back All
                throw;
            }
        }

        public async Task<bool> UpdateAsync(LotInfo lotInfo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.LotInfos.Update(lotInfo);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var lot = await _context.LotInfos.FindAsync(id);
            if (lot == null) return false;

            _context.LotInfos.Remove(lot);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}