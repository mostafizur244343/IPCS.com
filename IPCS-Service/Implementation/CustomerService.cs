using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IPCS_Service.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly IPCSDBContext _context;
        public CustomerService(IPCSDBContext context) { _context = context; }

        public async Task<string> GenerateCustomerCodeAsync()
        {
            // IgnoreQueryFilters Ignore Deleting Data and tracking Main ID
            var last = await _context.Customers.IgnoreQueryFilters().AsNoTracking().OrderByDescending(c => c.CustomerId).FirstOrDefaultAsync();
            int nextId = (last == null) ? 1 : last.CustomerId + 1;
            return $"CUST-{nextId:D5}";
        }

        public async Task<IEnumerable<Customer>> GetAllActiveAsync() =>
            await _context.Customers.AsNoTracking().ToListAsync();

        /// <summary>
        /// Server-side paging for the customer list. Same shape and ordering
        /// as GetAllActiveAsync, but only one page of rows is loaded.
        /// </summary>
        public async Task<PagedResult<Customer>> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.Customers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(c => c.CustomerName.ToLower().Contains(term)
                    || (c.Mobile != null && c.Mobile.ToLower().Contains(term))
                    || (c.CustomerCode != null && c.CustomerCode.ToLower().Contains(term)));
            }

            return await query.OrderBy(c => c.CustomerId).ToPagedResultAsync(page, pageSize);
        }

        public async Task<IEnumerable<Customer>> GetDeletedListAsync() =>
            await _context.Customers.IgnoreQueryFilters().Where(c => c.IsDeleted).AsNoTracking().ToListAsync();

        public async Task<bool> CreateAsync(Customer customer)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                customer.CustomerCode = await GenerateCustomerCodeAsync();
                
                // If opening is Due, set CurrentDue, else set AdvanceBalance
                if (customer.IsDue)
                {
                    customer.CurrentDue = customer.OpeningBalance;
                    customer.AdvanceBalance = 0;
                }
                else
                {
                    customer.AdvanceBalance = customer.OpeningBalance;
                    customer.CurrentDue = 0;
                }

                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch { await transaction.RollbackAsync(); throw; }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var cust = await _context.Customers.FindAsync(id);
            if (cust == null) return false;

            // Ordering guards: deleting a customer with money on their
            // account would hide the balance, and deleting one with live
            // sales would blank the customer lookup on the sales list and
            // the customer ledger. Settle the balance and delete the sales
            // first, then delete the customer.
            if (cust.CurrentDue != 0)
                throw new Exception($"Cannot delete customer {cust.CustomerName}: they have an outstanding balance of {cust.CurrentDue}. Settle the balance first, then delete the customer.");
            if (cust.AdvanceBalance != 0)
                throw new Exception($"Cannot delete customer {cust.CustomerName}: they have an advance balance of {cust.AdvanceBalance}. Settle the advance balance first, then delete the customer.");
            if (await _context.SalesMasters.AnyAsync(s => s.CustomerId == id))
                throw new Exception($"Cannot delete customer {cust.CustomerName}: they have existing sales records. Delete the sales first, then delete the customer.");

            cust.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var cust = await _context.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CustomerId == id);
            if (cust == null) return false;
            cust.IsDeleted = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Customer?> GetByIdAsync(int id) => await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == id);
        
        public async Task<Customer?> GetWithSalesHistoryAsync(int id) =>
            await _context.Customers
                .AsNoTracking()
                .Include(c => c.SalesMasters)
                    .ThenInclude(s => s.SalesDetails)
                .Include(c => c.SalesMasters)
                    .ThenInclude(s => s.Payments)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

        public async Task<bool> UpdateAsync(Customer customer)
        {
            try
            {
                _context.Entry(customer).State = EntityState.Modified;
                return await _context.SaveChangesAsync() > 0;
            }
            catch { throw; }
        }
    }
}