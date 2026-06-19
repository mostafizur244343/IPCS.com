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
    public class SupplierService : ISupplierService
    {
        private readonly IPCSDBContext _context;
        public SupplierService(IPCSDBContext context) { _context = context; }

        public async Task<string> GenerateSupplierCodeAsync()
        {
            try
            {
                var last = await _context.Suppliers.IgnoreQueryFilters().AsNoTracking().OrderByDescending(s => s.SupplierId).FirstOrDefaultAsync();
                int nextId = (last == null) ? 1 : last.SupplierId + 1;
                return $"SUP-{nextId:D4}";
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating supplier code: " + ex.Message);
            }
        }

        public async Task<IEnumerable<Supplier>> GetAllActiveAsync()
        {
            try
            {
                return await _context.Suppliers.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading active suppliers: " + ex.Message);
            }
        }

        public async Task<IEnumerable<Supplier>> GetDeletedListAsync()
        {
            try
            {
                return await _context.Suppliers.IgnoreQueryFilters().Where(s => s.IsDeleted).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading deleted suppliers: " + ex.Message);
            }
        }

        public async Task<bool> CreateAsync(Supplier supplier)
        {
            if (supplier == null) throw new Exception("Supplier data is missing.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if mobile already exists
                var exists = await _context.Suppliers.IgnoreQueryFilters().AnyAsync(s => s.Mobile == supplier.Mobile);
                if (exists) throw new Exception($"A supplier with mobile number '{supplier.Mobile}' already exists.");

                supplier.SupplierCode = await GenerateSupplierCodeAsync();
                supplier.CurrentDue = supplier.OpeningBalance;

                await _context.Suppliers.AddAsync(supplier);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            try
            {
                var sup = await _context.Suppliers.FindAsync(id);
                if (sup == null) throw new Exception("Supplier not found for deletion.");

                // Optional: Check dependencies (Purchases)
                var hasPurchases = await _context.PurchaseMasters.AnyAsync(p => p.SupplierId == id);
                if (hasPurchases) throw new Exception("Cannot delete supplier because they have existing purchase records.");

                sup.IsDeleted = true;
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> RestoreAsync(int id)
        {
            try
            {
                var sup = await _context.Suppliers.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SupplierId == id);
                if (sup == null) throw new Exception("Deleted supplier not found.");

                sup.IsDeleted = false;
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null) throw new Exception($"Supplier with ID {id} not found.");
                return supplier;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateAsync(Supplier supplier)
        {
            try
            {
                if (supplier == null) throw new Exception("Supplier data is missing.");

                var existing = await _context.Suppliers.FindAsync(supplier.SupplierId);
                if (existing == null) throw new Exception("Supplier not found for update.");

                // Check for duplicate mobile
                var exists = await _context.Suppliers.IgnoreQueryFilters().AnyAsync(s => s.Mobile == supplier.Mobile && s.SupplierId != supplier.SupplierId);
                if (exists) throw new Exception($"Another supplier with mobile number '{supplier.Mobile}' already exists.");

                _context.Entry(existing).CurrentValues.SetValues(supplier);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}