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
    public class CategoryService : ICategoryService
    {
        private readonly IPCSDBContext _context;

        public CategoryService(IPCSDBContext context)
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
                throw new Exception("Error loading categories: " + ex.Message);
            }
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                return category;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> CreateAsync(Category category)
        {
            try
            {
                if (category == null)
                {
                    throw new Exception("Category data is missing.");
                }

                // Check for duplicate name
                var exists = await _context.Categories.AnyAsync(c => c.CategoryName == category.CategoryName);
                if (exists)
                {
                    throw new Exception($"A category with the name '{category.CategoryName}' already exists.");
                }

                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            try
            {
                if (category == null) throw new Exception("Category data is missing.");

                var existing = await _context.Categories.FindAsync(category.CategoryId);
                if (existing == null) throw new Exception("Category not found for update.");

                // Check for duplicate name excluding current
                var exists = await _context.Categories.AnyAsync(c => c.CategoryName == category.CategoryName && c.CategoryId != category.CategoryId);
                if (exists)
                {
                    throw new Exception($"Another category with the name '{category.CategoryName}' already exists.");
                }

                _context.Entry(existing).CurrentValues.SetValues(category);
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
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    throw new Exception("Category not found for deletion.");
                }

                // Check if category is used by products before deleting
                var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
                if (hasProducts)
                {
                    throw new Exception("Cannot delete category because it is being used by one or more products.");
                }

                // Soft delete: the row stays in the DB (trash/restorable)
                // instead of being hard-removed.
                category.IsDeleted = true;
                _context.Categories.Update(category);
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
            var category = await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return false;
            if (!category.IsDeleted) return false;
            category.IsDeleted = false;
            _context.Categories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Category>> GetDeletedListAsync()
        {
            return await _context.Categories.IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
