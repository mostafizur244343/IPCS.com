using IPCS_Model.DTOs;
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
    public class BranchService : IBranchService
    {
        private readonly IPCSDBContext _context;

        public BranchService(IPCSDBContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateBranchCodeAsync()
        {
            try
            {
                var lastBranch = await _context.Branches.IgnoreQueryFilters().AsNoTracking().OrderByDescending(b => b.BranchId).FirstOrDefaultAsync();
                int nextId = (lastBranch == null) ? 1 : lastBranch.BranchId + 1;
                return $"BRANCH-{nextId:D3}";
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating branch code: " + ex.Message);
            }
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
                        PicturePath = b.PicturePath,
                        IsActive = b.IsActive,
                        TotalSentTransfers = 0,
                        TotalReceivedTransfers = 0,
                        TotalSentRequisitions = 0,
                        TotalReceivedRequisitions = 0,
                        TotalSales = 0,
                        TotalPurchases = 0
                    }).ToListAsync();
            }
            catch (Exception ex) 
            { 
                throw new Exception("Error loading branches: " + ex.Message); 
            }
        }

        public async Task<BranchResponseDTO?> GetByIdWithReportingAsync(int id)
        {
            try
            {
                var branch = await _context.Branches
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
                        PicturePath = b.PicturePath,
                        IsActive = b.IsActive,
                        TotalSentTransfers = b.SentTransfers.Count(),
                        TotalReceivedTransfers = b.ReceivedTransfers.Count(),
                        TotalSentRequisitions = b.SentRequisitions.Count(),
                        TotalReceivedRequisitions = b.ReceivedRequisitions.Count(),
                        TotalSales = b.SalesMasters.Count(),
                        TotalPurchases = b.PurchaseMasters.Count()
                    }).FirstOrDefaultAsync();

                if (branch == null) throw new Exception($"Branch with ID {id} not found.");
                return branch;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }

        public async Task<Branch?> GetByIdAsync(int id)
        {
            try 
            { 
                var branch = await _context.Branches.FindAsync(id);
                if (branch == null) throw new Exception($"Branch with ID {id} not found.");
                return branch;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }

        public async Task<bool> CreateAsync(Branch branch)
        {
            try
            {
                var exists = await _context.Branches.AnyAsync(b => b.BranchName.ToLower() == branch.BranchName.ToLower());
                if (exists) throw new Exception($"Branch with name '{branch.BranchName}' already exists.");

                branch.BranchCode = await GenerateBranchCodeAsync();
                await _context.Branches.AddAsync(branch);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }

        public async Task<bool> UpdateAsync(Branch branch)
        {
            try
            {
                var exists = await _context.Branches.AnyAsync(b => b.BranchName.ToLower() == branch.BranchName.ToLower() && b.BranchId != branch.BranchId);
                if (exists) throw new Exception($"Another branch with name '{branch.BranchName}' already exists.");

                _context.Branches.Update(branch);
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
                var branch = await _context.Branches.FindAsync(id);
                if (branch == null) throw new Exception(" Branch not found for deletion.");

                // Check for dependencies (Stocks, Users, Sales, Purchases)
                var hasStock = await _context.BranchLotStocks.AnyAsync(s => s.BranchId == id && s.CurrentStock > 0);
                if (hasStock) throw new Exception("Cannot delete branch because it has existing stock.");

                _context.Branches.Remove(branch);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message); 
            }
        }
    }
}