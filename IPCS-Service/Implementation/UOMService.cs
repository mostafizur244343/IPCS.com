using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;

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
                return await _context.UOMs.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("UOM Not Found" + ex.Message);
            }
        }

        public async Task<UOM?> GetByIdAsync(int id)
        {
            try
            {
                var uom = await _context.UOMs.FindAsync(id);
                if (uom == null) throw new Exception("No UOM in this ID");
                return uom;
            }
            catch (Exception ex)
            {
                throw new Exception("UOM Finding Error... " + ex.Message);
            }
        }

        public async Task<bool> CreateAsync(UOM uom)
        {
            try
            {
                if (uom == null) throw new Exception("Pleasae input required field");
                await _context.UOMs.AddAsync(uom);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("UOM Creating Error " + ex.Message);
            }
        }

        public async Task<bool> UpdateAsync(UOM uom)
        {
            try
            {
                _context.UOMs.Update(uom);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("UOM Updating Error... " + ex.Message);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var uom = await _context.UOMs.FindAsync(id);
                if (uom == null) throw new Exception(" UOM Already Deleted or not Created");
                _context.UOMs.Remove(uom);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("UOM Deleting Error... " + ex.Message);
            }
        }
    }
}
