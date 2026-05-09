using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;

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
            try { return await _context.GenericInfos.ToListAsync(); }
            catch (Exception ex) { throw new Exception("Loading Generic List Error " + ex.Message); }
        }

        public async Task<GenericInfo?> GetByIdAsync(int id)
        {
            try
            {
                var info = await _context.GenericInfos.FindAsync(id);
                if (info == null) throw new Exception("Error to find this Information");
                return info;
            }
            catch (Exception ex) { throw new Exception("Finding Information Error... " + ex.Message); }
        }

        public async Task<bool> CreateAsync(GenericInfo genericInfo)
        {
            try
            {
                await _context.GenericInfos.AddAsync(genericInfo);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Creating Error... " + ex.Message); }
        }

        public async Task<bool> UpdateAsync(GenericInfo genericInfo)
        {
            try
            {
                _context.GenericInfos.Update(genericInfo);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Error Updating Info... " + ex.Message); }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var info = await _context.GenericInfos.FindAsync(id);
                if (info == null) throw new Exception("Info Not found to Delete");
                _context.GenericInfos.Remove(info);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Error Deleting... " + ex.Message); }
        }
    }
}