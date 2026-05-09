using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCS_Service.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IPCSDBContext _context;

        public CategoryService (IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            try
            {
                return await _context.Categories.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Load Category List Error" + ex.Message);
            }
        }

        public async Task<Category?> GetByIdAsync (int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    throw new Exception("Do not Find any Category");
                }
                return category;
            }
            catch (Exception ex)
            {
                throw new Exception("Category ID not found " + ex.Message);
            }
        }

        public async Task<bool> CreateAsync (Category category)
        {
            try
            {
                if (category == null)
                {
                    throw new Exception("Please enter a category");
                }
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Creating Category... Please try again");
            }
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            try
            {
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Category Update" + ex.Message);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    throw new Exception("Do not find this ID");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error to delete Category..." + ex.Message);
            }
        }
    }
}
