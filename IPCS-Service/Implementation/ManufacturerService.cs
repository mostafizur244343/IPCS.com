using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPCS_Service.Implementation
{
    public class ManufacturerService : IManufacturerService
    {
        private readonly IPCSDBContext _context;

        public ManufacturerService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Manufacturer>> GetAllAsync()
        {
            try
            {
                return await _context.Manufacturers.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading manufacturers: " + ex.Message);
            }
        }

        public async Task<Manufacturer?> GetByIdAsync(int id)
        {
            try
            {
                var manufacturer = await _context.Manufacturers.FindAsync(id);
                return manufacturer;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> CreateAsync(Manufacturer manufacturer)
        {
            try
            {
                if (manufacturer == null) throw new Exception("Manufacturer data is missing.");

                var exists = await _context.Manufacturers.AnyAsync(m => m.BrandName == manufacturer.BrandName);
                if (exists) throw new Exception($"A manufacturer with the name '{manufacturer.BrandName}' already exists.");

                await _context.Manufacturers.AddAsync(manufacturer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateAsync(Manufacturer manufacturer)
        {
            try
            {
                if (manufacturer == null) throw new Exception("Manufacturer data is missing.");

                var existing = await _context.Manufacturers.FindAsync(manufacturer.BrandId);
                if (existing == null) throw new Exception("Manufacturer not found for update.");

                var exists = await _context.Manufacturers.AnyAsync(m => m.BrandName == manufacturer.BrandName && m.BrandId != manufacturer.BrandId);
                if (exists) throw new Exception($"Another manufacturer with the name '{manufacturer.BrandName}' already exists.");

                _context.Entry(existing).CurrentValues.SetValues(manufacturer);
                await _context.SaveChangesAsync();
                return true;
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
                var manufacturer = await _context.Manufacturers.FindAsync(id);
                if (manufacturer == null) throw new Exception("Manufacturer not found for deletion.");

                var hasProducts = await _context.Products.AnyAsync(p => p.BrandId == id);
                if (hasProducts) throw new Exception("Cannot delete manufacturer because it is being used by one or more products.");

                // Soft delete: the row stays in the DB (trash/restorable)
                // instead of being hard-removed.
                manufacturer.IsDeleted = true;
                _context.Manufacturers.Update(manufacturer);
                await _context.SaveChangesAsync();
                return true;
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
            var manufacturer = await _context.Manufacturers.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.BrandId == id);
            if (manufacturer == null) return false;
            if (!manufacturer.IsDeleted) return false;
            manufacturer.IsDeleted = false;
            _context.Manufacturers.Update(manufacturer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Manufacturer>> GetDeletedListAsync()
        {
            return await _context.Manufacturers.IgnoreQueryFilters()
                .Where(m => m.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
