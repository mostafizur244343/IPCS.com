using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class BranchService : IBranchService
    {
        private readonly IPCSDBContext _context;

        public BranchService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateBranchCodeAsync()
        {
            var lastBranch = await _context.Branches.AsNoTracking().OrderByDescending(b => b.BranchId).FirstOrDefaultAsync();
            int nextId = (lastBranch == null) ? 1 : lastBranch.BranchId + 1;
            return $"BRANCH-{nextId:D3}"; // Result BRANCH-001, BRANCH-002...
        }

        public async Task<IEnumerable<BranchResponseDTO>> GetAllAsync()
        {
            try
            {
                return await _context.Branches
                    .AsNoTracking()
                    .Select(b => new BranchResponseDTO
                    {
                        BranchId = b.BranchId,
                        BranchName = b.BranchName,
                        BranchCode = b.BranchCode,
                        Address = b.Address,
                        ContactNumber = b.ContactNumber,
                        Email = b.Email,
                        ManagerName = b.ManagerName,
                        IsActive = b.IsActive,
                        TotalSentTransfers = b.SentTransfers.Count(),
                        TotalReceivedTransfers = b.ReceivedTransfers.Count(),
                        TotalSentRequisitions = b.SentRequisitions.Count(),
                        TotalReceivedRequisitions = b.ReceivedRequisitions.Count(),
                        TotalSales = b.SalesMasters.Count(),
                        TotalPurchases = b.PurchaseMasters.Count()
                    }).ToListAsync();
            }
            catch (Exception ex) { throw new Exception("Loading Branch List Error... " + ex.Message); }
        }

        public async Task<BranchResponseDTO?> GetByIdWithReportingAsync(int id)
        {
            try
            {
                return await _context.Branches
                    .AsNoTracking()
                    .Where(b => b.BranchId == id)
                    .Select(b => new BranchResponseDTO
                    {
                        BranchId = b.BranchId,
                        BranchName = b.BranchName,
                        BranchCode = b.BranchCode,
                        Address = b.Address,
                        ContactNumber = b.ContactNumber,
                        Email = b.Email,
                        ManagerName = b.ManagerName,
                        IsActive = b.IsActive,
                        TotalSentTransfers = b.SentTransfers.Count(),
                        TotalReceivedTransfers = b.ReceivedTransfers.Count(),
                        TotalSentRequisitions = b.SentRequisitions.Count(),
                        TotalReceivedRequisitions = b.ReceivedRequisitions.Count(),
                        TotalSales = b.SalesMasters.Count(),
                        TotalPurchases = b.PurchaseMasters.Count()
                    }).FirstOrDefaultAsync();
            }
            catch (Exception ex) { throw new Exception("Finding Branch Error... " + ex.Message); }
        }

        public async Task<Branch?> GetByIdAsync(int id)
        {
            try { return await _context.Branches.FindAsync(id); }
            catch (Exception ex) { throw new Exception("Finding Branch Error... " + ex.Message); }
        }

        public async Task<bool> CreateAsync(Branch branch)
        {
            try
            {
                branch.BranchCode = await GenerateBranchCodeAsync(); // Auto Genereting Code
                await _context.Branches.AddAsync(branch);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Save Error... " + ex.Message); }
        }

        public async Task<bool> UpdateAsync(Branch branch)
        {
            try
            {
                _context.Branches.Update(branch);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Updating Error... " + ex.Message); }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var branch = await _context.Branches.FindAsync(id);
                if (branch == null) return false;
                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { throw new Exception("Deleting Error... " + ex.Message); }
        }
    }
}