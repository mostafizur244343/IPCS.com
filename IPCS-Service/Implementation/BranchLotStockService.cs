using IPCS_Model.DTOs;
using IPCS_Model.Entities;
using IPCS_Repo.Data;
using IPCS_Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class BranchLotStockService : IBranchLotStockService
    {
        private readonly IPCSDBContext _context;

        public BranchLotStockService(IPCSDBContext context)
        {
            _context = context;
        }

        // For the fast data Loading Using AsNoTracking 
        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.BranchLotStocks
                .AsNoTracking()
                .Include(s => s.Product)
                .Include(s => s.Branch)
                .Select(s => new {
                    s.Branch!.BranchName,
                    s.Product!.ProductName,
                    s.Lot!.LotNumber,
                    s.CurrentStock,
                    s.DamagedStock,
                    s.LastUpdated
                }).ToListAsync();
        }

        //Low Stock Alert Logic (Where CurrentStock < Product.ReorderLevel)
        public async Task<IEnumerable<object>> GetLowStockAlertAsync(int branchId)
        {
            return await _context.BranchLotStocks
                .AsNoTracking()
                .Include(s => s.Product)
                .Where(s => s.BranchId == branchId && s.CurrentStock <= s.Product!.ReorderLevel)
                .Select(s => new {
                    s.Product!.ProductName,
                    s.CurrentStock,
                    s.Product.ReorderLevel
                }).ToListAsync();
        }

        // Stock Adjustment Method ( Using Transaction )
        public async Task<bool> AdjustStockAsync(BranchLotStockDTO model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var stock = await _context.BranchLotStocks
                    .FirstOrDefaultAsync(s => s.BranchId == model.BranchId && s.ProductId == model.ProductId && s.LotId == model.LotId);

                if (stock == null)
                {
                    // New Entry when no stock found (Work as Trigger )
                    stock = new BranchLotStock
                    {
                        BranchId = model.BranchId,
                        ProductId = model.ProductId,
                        LotId = model.LotId,
                        CurrentStock = model.Quantity,
                        DamagedStock = model.DamagedQuantity,
                        LastUpdated = DateTime.Now
                    };
                    await _context.BranchLotStocks.AddAsync(stock);
                }
                else
                {
                    // Update If get stock ( Use Entry State)
                    stock.CurrentStock = model.Quantity;
                    stock.DamagedStock = model.DamagedQuantity;
                    stock.LastUpdated = DateTime.Now;
                    _context.Entry(stock).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); //If Transection Successfull
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(); // Cancel Transection
                return false;
            }
        }

        // Internal Call On Purchase or Sale
        public async Task<bool> UpdateStockInternalAsync(int branchId, int productId, int lotId, decimal quantity, bool isAddition)
        {
            var stock = await _context.BranchLotStocks
                .FirstOrDefaultAsync(s => s.BranchId == branchId && s.ProductId == productId && s.LotId == lotId);

            if (stock == null && isAddition)
            {
                stock = new BranchLotStock { BranchId = branchId, ProductId = productId, LotId = lotId, CurrentStock = quantity };
                await _context.BranchLotStocks.AddAsync(stock);
            }
            else if (stock != null)
            {
                if (isAddition) stock.CurrentStock += quantity;
                else stock.CurrentStock -= quantity;

                stock.LastUpdated = DateTime.Now;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<object?> GetStockDetailAsync(int branchId, int productId)
        {
            return await _context.BranchLotStocks
                .AsNoTracking()
                .Where(s => s.BranchId == branchId && s.ProductId == productId)
                .SumAsync(s => s.CurrentStock);
        }
    }
}