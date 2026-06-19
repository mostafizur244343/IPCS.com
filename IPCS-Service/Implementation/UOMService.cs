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
    public class UOMService : IUOMService
    {
        private readonly IPCSDBContext _context;

        public UOMService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UOM>> GetAllAsync()
        {
            try
            {
                return await _context.UOMs.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading UOM list: " + ex.Message);
            }
        }

        public async Task<UOM?> GetByIdAsync(int id)
        {
            try
            {
                var uom = await _context.UOMs.FindAsync(id);
                if (uom == null) throw new Exception($"UOM with ID {id} not found.");
                return uom;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> CreateAsync(UOM uom)
        {
            try
            {
                if (uom == null) throw new Exception("UOM data is missing.");
                
                var exists = await _context.UOMs.AnyAsync(u => u.UOMName == uom.UOMName);
                if (exists) throw new Exception($"A UOM with the name '{uom.UOMName}' already exists.");

                await _context.UOMs.AddAsync(uom);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateAsync(UOM uom)
        {
            try
            {
                if (uom == null) throw new Exception("UOM data is missing.");
                
                var existing = await _context.UOMs.FindAsync(uom.UOMId);
                if (existing == null) throw new Exception("UOM not found for update.");

                var exists = await _context.UOMs.AnyAsync(u => u.UOMName == uom.UOMName && u.UOMId != uom.UOMId);
                if (exists) throw new Exception($"Another UOM with the name '{uom.UOMName}' already exists.");

                _context.Entry(existing).CurrentValues.SetValues(uom);
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
                var uom = await _context.UOMs.FindAsync(id);
                if (uom == null) throw new Exception("UOM not found for deletion.");

                // Check dependencies (e.g., Products or Unit Conversions)
                var hasProducts = await _context.Products.AnyAsync(p => p.BaseUOMId == id);
                if (hasProducts) throw new Exception("Cannot delete UOM because it is being used by one or more products.");

                _context.UOMs.Remove(uom);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
