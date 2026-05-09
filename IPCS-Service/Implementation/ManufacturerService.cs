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
    public class ManufacturerService : IManufacturerService
    {
        private readonly IPCSDBContext _context;

        public ManufacturerService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Manufacturer>> GetAllAsync()
        {
            try { return await _context.Manufacturers.ToListAsync(); }
            catch (Exception ex) { throw new Exception("Error to Loading Manufacturers List " + ex.Message); }
        }

        public async Task<Manufacturer?> GetByIdAsync(int id)
        {
            try
            {
                var brand = await _context.Manufacturers.FindAsync(id);
                if (brand == null) throw new Exception("Don't Find any manufaturer in this id");
                return brand;
            }
            catch (Exception ex) { throw new Exception("Error to Find information " + ex.Message); }
        }

        public async Task<bool> CreateAsync(Manufacturer manufacturer)
        {
            try
            {
                await _context.Manufacturers.AddAsync(manufacturer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Error to save New informatiomn " + ex.Message); }
        }

        public async Task<bool> UpdateAsync(Manufacturer manufacturer)
        {
            try
            {
                _context.Manufacturers.Update(manufacturer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Error to Update Information " + ex.Message); }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var brand = await _context.Manufacturers.FindAsync(id);
                if (brand == null) throw new Exception("Don't find any Info for Delete");
                _context.Manufacturers.Remove(brand);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Error to delete this Informatinon " + ex.Message); }
        }
    }
}