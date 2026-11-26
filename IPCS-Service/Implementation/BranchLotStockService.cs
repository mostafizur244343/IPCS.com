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
        private readonly IProductService _productService;

        public BranchLotStockService(IPCSDBContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        // For the fast data Loading Using AsNoTracking 
        public async Task<IEnumerable<object>> GetAllAsync()
        {
            var stocks = await _context.BranchLotStocks
                .AsNoTracking()
                .Include(s => s.Product)
                .Include(s => s.Product!.UOM)
                .Include(s => s.Branch)
                .Include(s => s.Lot)
                .ToListAsync();

            var result = new List<object>();
            foreach (var s in stocks)
            {
                var displayStock = await _productService.ConvertFromBaseUnit(s.ProductId, s.Product!.UOMId, s.CurrentStock);
                result.Add(new {
                    s.BranchId,
                    s.Branch!.BranchName,
                    s.ProductId,
                    s.Product!.ProductName,
                    SKU = s.Product!.SKU ?? s.Product!.ProductCode,
                    UomId = s.Product!.UOMId,
                    UomName = s.Product!.UOM!.UOMName,
                    s.LotId,
                    s.Lot!.LotNumber,
                    CurrentStock = displayStock,
                    CurrentStockInPcs = s.CurrentStock,
                    s.DamagedStock,
                    PicturePath = s.Product!.PicturePath,
                    s.LastUpdated,
                    PurchasePrice = s.Lot?.PurchasePrice ?? 0
                });
            }
            return result;
        }

        /// <summary>
        /// Server-side paging for branch stock. Same shape as GetAllAsync,
        /// but only one page of rows is loaded — and the unit conversion that
        /// GetAllAsync runs once per row (one ProductUnitConversions query
        /// per stock row, the N+1) is done here with ONE batched query for
        /// all distinct products on the page.
        /// </summary>
        public async Task<PagedResult<BranchLotStockListItem>> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.BranchLotStocks.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(s => (s.Product != null && s.Product.ProductName.ToLower().Contains(term))
                    || (s.Product != null && (s.Product.SKU ?? string.Empty).ToLower().Contains(term))
                    || (s.Product != null && (s.Product.ProductCode ?? string.Empty).ToLower().Contains(term))
                    || (s.Lot != null && s.Lot.LotNumber.ToLower().Contains(term))
                    || (s.Branch != null && s.Branch.BranchName.ToLower().Contains(term)));
            }

            var total = await query.CountAsync();

            var pageStocks = await query
                .Include(s => s.Product)
                .Include(s => s.Product!.UOM)
                .Include(s => s.Branch)
                .Include(s => s.Lot)
                .OrderByDescending(s => s.StockId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // One batched conversion lookup for the whole page (see
            // ProductService.ConvertFromBaseUnit for the per-row original).
            var productIds = pageStocks.Select(s => s.ProductId).Distinct().ToList();
            var conversionsByProduct = (await _context.ProductUnitConversions
                    .AsNoTracking()
                    .Where(c => productIds.Contains(c.ProductId))
                    .OrderByDescending(c => c.Level)
                    .ToListAsync())
                .GroupBy(c => c.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var items = pageStocks.Select(s => new BranchLotStockListItem
            {
                BranchId = s.BranchId,
                BranchName = s.Branch?.BranchName,
                ProductId = s.ProductId,
                ProductName = s.Product?.ProductName,
                SKU = s.Product != null ? (s.Product.SKU ?? s.Product.ProductCode) : null,
                UomId = s.Product?.UOMId ?? 0,
                UomName = s.Product?.UOM?.UOMName,
                LotId = s.LotId,
                LotNumber = s.Lot?.LotNumber,
                CurrentStock = ConvertFromBaseUnitInMemory(s.Product, s.Product?.UOMId ?? 0, s.CurrentStock, conversionsByProduct),
                CurrentStockInPcs = s.CurrentStock,
                DamagedStock = s.DamagedStock,
                PicturePath = s.Product?.PicturePath,
                LastUpdated = s.LastUpdated,
                PurchasePrice = s.Lot?.PurchasePrice ?? 0
            }).ToList();

            return new PagedResult<BranchLotStockListItem>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// In-memory copy of ProductService.ConvertFromBaseUnit, operating on
        /// the pre-fetched conversion lists so no per-row query is needed.
        /// </summary>
        private static decimal ConvertFromBaseUnitInMemory(Product? product, int targetUnitId, decimal quantity,
            Dictionary<int, List<ProductUnitConversion>> conversionsByProduct)
        {
            if (product == null) return quantity;
            if (product.BaseUOMId == targetUnitId) return quantity;
            if (!conversionsByProduct.TryGetValue(product.ProductId, out var conversions)) return quantity;

            decimal finalQty = quantity;
            bool foundTarget = false;
            foreach (var conv in conversions)
            {
                if (conv.ToUnitId == targetUnitId || foundTarget)
                {
                    finalQty /= conv.Factor;
                    foundTarget = true;
                }
            }
            return finalQty;
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

        public async Task<IEnumerable<object>> GetActiveLotsByProductAndBranchAsync(int productId, int branchId)
        {
            return await _context.BranchLotStocks
                .AsNoTracking()
                .Where(s => s.BranchId == branchId && s.ProductId == productId && s.CurrentStock > 0)
                .Include(s => s.Lot)
                .Select(s => new
                {
                    s.LotId,
                    s.Lot!.LotNumber,
                    s.Lot.ExpiryDate,
                    s.CurrentStock,
                    s.Lot.PurchasePrice
                })
                .OrderBy(s => s.ExpiryDate) // Show soon-to-expire lots first
                .ToListAsync();
        }
    }
}