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
                if (loc == null) throw new Exception($"Store location with ID {id} not found.");
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

                _context.StoreLocations.Remove(loc);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }
    }
}