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

                // Ordering guard: a unit used by live unit conversions
                // cannot be deleted — the conversions look both units up
                // by id for quantity math. Delete the conversions first.
                var hasConversions = await _context.ProductUnitConversions.AnyAsync(c => c.FromUnitId == id || c.ToUnitId == id);
                if (hasConversions) throw new Exception("Cannot delete UOM because it is used by one or more unit conversions.");

                // Soft delete: the row stays in the DB (trash/restorable)
                // instead of being hard-removed.
                uom.IsDeleted = true;
                _context.UOMs.Update(uom);
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
            var uom = await _context.UOMs.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UOMId == id);
            if (uom == null) return false;
            if (!uom.IsDeleted) return false;
            uom.IsDeleted = false;
            _context.UOMs.Update(uom);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<UOM>> GetDeletedListAsync()
        {
            return await _context.UOMs.IgnoreQueryFilters()
                .Where(u => u.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
