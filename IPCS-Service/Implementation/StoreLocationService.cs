using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            try 
            { 
                return await _context.StoreLocations.AsNoTracking().ToListAsync(); 
            }
            catch (Exception ex) 
            { 
                throw new Exception("Error loading store locations: " + ex.Message); 
            }
        }

        public async Task<StoreLocation?> GetByIdAsync(int id)
        {
            try
            {
                var loc = await _context.StoreLocations.FindAsync(id);
                return loc;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }

        public async Task<bool> CreateAsync(StoreLocation location)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(location.ShelfName)) throw new Exception("Shelf name is required.");

                await _context.StoreLocations.AddAsync(location);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }

        public async Task<bool> UpdateAsync(StoreLocation location)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(location.ShelfName)) throw new Exception("Shelf name is required.");

                _context.StoreLocations.Update(location);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var loc = await _context.StoreLocations.FindAsync(id);
                if (loc == null) throw new Exception("Store location not found for deletion.");

                // Check for dependencies (Products)
                var hasProducts = await _context.Products.AnyAsync(p => p.LocationId == id && !p.IsDeleted);
                if (hasProducts) throw new Exception("Cannot delete this location because it is assigned to active products.");

                // Soft delete: the row stays in the DB (trash/restorable)
                // instead of being hard-removed.
                loc.IsDeleted = true;
                _context.StoreLocations.Update(loc);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }

        // Master data carries no books effects, so restore only flips the
        // flag back.
        public async Task<bool> RestoreAsync(int id)
        {
            var loc = await _context.StoreLocations.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.LocationId == id);
            if (loc == null) return false;
            if (!loc.IsDeleted) return false;
            loc.IsDeleted = false;
            _context.StoreLocations.Update(loc);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<StoreLocation>> GetDeletedListAsync()
        {
            return await _context.StoreLocations.IgnoreQueryFilters()
                .Where(l => l.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
