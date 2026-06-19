using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Implementation
{
    public class GenericInfoService : IGenericInfoService
    {
        private readonly IPCSDBContext _context;

        public GenericInfoService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GenericInfo>> GetAllAsync()
        {
            try 
            { 
                return await _context.GenericInfos.AsNoTracking().ToListAsync(); 
            }
            catch (Exception ex) 
            { 
                throw new Exception("Error loading generic information: " + ex.Message); 
            }
        }

        public async Task<GenericInfo?> GetByIdAsync(int id)
        {
            try
            {
                var info = await _context.GenericInfos.FindAsync(id);
                if (info == null) throw new Exception($"Generic info with ID {id} not found.");
                return info;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }

        public async Task<bool> CreateAsync(GenericInfo genericInfo)
        {
            try
            {
                var exists = await _context.GenericInfos.AnyAsync(g => g.GenericName.ToLower() == genericInfo.GenericName.ToLower());
                if (exists) throw new Exception($"Generic name '{genericInfo.GenericName}' already exists.");

                await _context.GenericInfos.AddAsync(genericInfo);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }

        public async Task<bool> UpdateAsync(GenericInfo genericInfo)
        {
            try
            {
                var exists = await _context.GenericInfos.AnyAsync(g => g.GenericName.ToLower() == genericInfo.GenericName.ToLower() && g.GenericId != genericInfo.GenericId);
                if (exists) throw new Exception($"Generic name '{genericInfo.GenericName}' already exists.");

                _context.GenericInfos.Update(genericInfo);
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
                var info = await _context.GenericInfos.FindAsync(id);
                if (info == null) throw new Exception("Information not found for deletion.");

                // Check for dependencies (Products)
                var hasProducts = await _context.Products.AnyAsync(p => p.GenericId == id && !p.IsDeleted);
                if (hasProducts) throw new Exception("Cannot delete this generic info because it is assigned to active products.");

                _context.GenericInfos.Remove(info);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }
    }
}