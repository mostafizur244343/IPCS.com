using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class StoreLocationService : IStoreLocationService
    {
        private readonly IPCSDBContext _context;

        public StoreLocationService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StoreLocation>> GetAllAsync()
        {
            try { return await _context.StoreLocations.ToListAsync(); }
            catch (Exception ex) { throw new Exception("Finding Location List Error... " + ex.Message); }
        }

        public async Task<StoreLocation?> GetByIdAsync(int id)
        {
            try
            {
                var loc = await _context.StoreLocations.FindAsync(id);
                if (loc == null) throw new Exception("Not found in this Location...");
                return loc;
            }
            catch (Exception ex) { throw new Exception("Find by Id Error " + ex.Message); }
        }

        public async Task<bool> CreateAsync(StoreLocation location)
        {
            try
            {
                await _context.StoreLocations.AddAsync(location);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Saving Error... " + ex.Message); }
        }

        public async Task<bool> UpdateAsync(StoreLocation location)
        {
            try
            {
                _context.StoreLocations.Update(location);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Error Updating " + ex.Message); }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var loc = await _context.StoreLocations.FindAsync(id);
                if (loc == null) throw new Exception("Not Found any info for delete");
                _context.StoreLocations.Remove(loc);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Deleting Errore..." + ex.Message); }
        }
    }
}