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
    public class ProductService : IProductService
    {
        private readonly IPCSDBContext _context;
        public ProductService(IPCSDBContext context) { _context = context; }

        public async Task<string> GenerateProductCodeAsync()
        {
            try
            {
                var last = await _context.Products.IgnoreQueryFilters().AsNoTracking().OrderByDescending(p => p.ProductId).FirstOrDefaultAsync();
                int nextId = (last == null) ? 1 : last.ProductId + 1;
                return $"PROD-{nextId:D5}";
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating product code: " + ex.Message);
            }
        }

        public async Task<decimal> ConvertToBaseUnit(int productId, int selectedUnitId, decimal quantity)
        {
            try
            {
                var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null) return quantity;

                if (product.BaseUOMId == selectedUnitId) return quantity;

                var conversions = await _context.ProductUnitConversions
                                      .AsNoTracking()
                                      .Where(p => p.ProductId == productId)
                                      .OrderBy(p => p.Level)
                                      .ToListAsync();

                decimal finalQty = quantity;
                bool foundStart = false;

                foreach (var conv in conversions)
                {
                    if (conv.FromUnitId == selectedUnitId || foundStart)
                    {
                        finalQty *= conv.Factor;
                        foundStart = true;
                    }
                }
                return finalQty;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in unit conversion to base: " + ex.Message);
            }
        }

        public async Task<decimal> ConvertFromBaseUnit(int productId, int targetUnitId, decimal quantity)
        {
            try
            {
                var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null) return quantity;

                if (product.BaseUOMId == targetUnitId) return quantity;

                var conversions = await _context.ProductUnitConversions
                                      .AsNoTracking()
                                      .Where(p => p.ProductId == productId)
                                      .OrderByDescending(p => p.Level)
                                      .ToListAsync();

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
            catch (Exception ex)
            {
                throw new Exception("Error in unit conversion from base: " + ex.Message);
            }
        }

        public async Task<bool> CreateAsync(Product product, decimal openingQty, decimal openingCost, int selectedUnitId, string? newGenericName = null, string? newCategoryName = null, string? newBrandName = null, string? newLocationName = null, string? newUomName = null, string? userName = null)
        {
            if (product == null) throw new Exception("Product data is missing.");

            // A product needs a unit of measure: either an existing UOM
            // (UOMId) or a new unit name to create one. Without it the
            // insert fails with a bare EF error (UOMId 0 → FK violation).
            // Validate up front so the caller gets a clear message.
            if (product.UOMId <= 0 && string.IsNullOrWhiteSpace(newUomName))
                throw new Exception("UOM is required to create a product. Please select a unit of measure.");
            if (product.UOMId > 0 && !await _context.UOMs.AnyAsync(u => u.UOMId == product.UOMId))
                throw new Exception($"UOM with ID {product.UOMId} does not exist. Please select a valid unit of measure.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if Product Name already exists (case-insensitive)
                var nameExists = await _context.Products.AnyAsync(p => p.ProductName.ToLower() == product.ProductName.ToLower() && !p.IsDeleted);
                if (nameExists) throw new Exception($"A product with name '{product.ProductName}' already exists.");

                if (!string.IsNullOrWhiteSpace(product.SKU))
                {
                    var skuExists = await _context.Products.AnyAsync(p => p.SKU == product.SKU && !p.IsDeleted);
                    if (skuExists) throw new Exception("This SKU is already assigned to another product.");
                }

                // Handling sub-entity creation (Common Logic)
                if (!string.IsNullOrWhiteSpace(newGenericName))
                {
                    var generic = new GenericInfo { GenericName = newGenericName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.GenericInfos.AddAsync(generic);
                    await _context.SaveChangesAsync();
                    product.GenericId = generic.GenericId;
                }
                if (!string.IsNullOrWhiteSpace(newCategoryName))
                {
                    var category = new Category { CategoryName = newCategoryName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.Categories.AddAsync(category);
                    await _context.SaveChangesAsync();
                    product.CategoryId = category.CategoryId;
                }
                if (!string.IsNullOrWhiteSpace(newBrandName))
                {
                    var brand = new Manufacturer { BrandName = newBrandName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.Manufacturers.AddAsync(brand);
                    await _context.SaveChangesAsync();
                    product.BrandId = brand.BrandId;
                }
                if (!string.IsNullOrWhiteSpace(newLocationName))
                {
                    var location = new StoreLocation { ShelfName = newLocationName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.StoreLocations.AddAsync(location);
                    await _context.SaveChangesAsync();
                    product.LocationId = location.LocationId;
                }
                if (!string.IsNullOrWhiteSpace(newUomName))
                {
                    var uom = new UOM { UOMName = newUomName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.UOMs.AddAsync(uom);
                    await _context.SaveChangesAsync();
                    product.UOMId = uom.UOMId;
                    product.BaseUOMId = uom.UOMId;
                    selectedUnitId = uom.UOMId;
                }

                product.ProductCode = await GenerateProductCodeAsync();
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                decimal baseQty = await ConvertToBaseUnit(product.ProductId, selectedUnitId, openingQty);

                if (product.IsService)
                {
                    product.CurrentStock = 0;
                    product.CostPrice = 0;
                }
                else
                {
                    decimal totalOpeningCost = openingQty * openingCost;
                    product.CurrentStock = baseQty;
                    product.CostPrice = (baseQty > 0) ? totalOpeningCost / baseQty : openingCost;

                    if (openingQty > 0)
                    {
                        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.IsActive);
                        int branchId = branch?.BranchId ?? 1;

                        var lot = new LotInfo
                        {
                            ProductId = product.ProductId,
                            LotNumber = "LOT-OPENING",
                            ExpiryDate = DateTime.Now.AddYears(5),
                            PurchasePrice = product.CostPrice,
                            IsActive = true
                        };
                        await _context.LotInfos.AddAsync(lot);
                        await _context.SaveChangesAsync();

                        var branchLotStock = new BranchLotStock
                        {
                            BranchId = branchId,
                            ProductId = product.ProductId,
                            LotId = lot.LotId,
                            CurrentStock = baseQty,
                            LastUpdated = DateTime.Now
                        };
                        await _context.BranchLotStocks.AddAsync(branchLotStock);

                        var ledger = new StockLedger
                        {
                            BranchId = branchId,
                            ProductId = product.ProductId,
                            LotId = lot.LotId,
                            TransactionType = "Opening Stock",
                            ReferenceNo = product.ProductCode,
                            TransactionDate = DateTime.Now,
                            PreviousBalance = 0,
                            QuantityIn = baseQty,
                            QuantityOut = 0
                        };
                        await _context.StockLedgers.AddAsync(ledger);
                    }
                }

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

        public async Task UpdateMovingAverageCostAsync(int productId, decimal newQtyInPcs, decimal newUnitPriceInPcs)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return;

                decimal totalValue = (product.CurrentStock * product.CostPrice) + (newQtyInPcs * newUnitPriceInPcs);
                decimal totalQty = product.CurrentStock + newQtyInPcs;

                if (totalQty > 0)
                    product.CostPrice = Math.Round(totalValue / totalQty, 2);

                product.CurrentStock = totalQty;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating moving average cost: " + ex.Message);
            }
        }

        public async Task<IEnumerable<Product>> GetAllActiveAsync(string? search = null, int? branchId = null)
        {
            try
            {
                var products = await BuildActiveQuery(search, branchId)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.UOM)
                    .Include(p => p.BaseUOM)
                    .Include(p => p.Generic)
                    .ToListAsync();

                // If branchId is provided, override CurrentStock with branch-specific stock from BranchLotStocks
                if (branchId.HasValue)
                {
                    await ApplyBranchStockAsync(products, branchId.Value);
                }

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading products: " + ex.Message);
            }
        }

        /// <summary>
        /// Server-side paging for the product list. Same filter as
        /// GetAllActiveAsync, but only one page of rows (with their lookups)
        /// is loaded. Branch stock is resolved with ONE grouped query for the
        /// page instead of one SumAsync per product (the old N+1).
        /// </summary>
        public async Task<PagedResult<Product>> GetPagedAsync(string? search, int? branchId, int page, int pageSize)
        {
            try
            {
                var query = BuildActiveQuery(search, branchId)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.UOM)
                    .Include(p => p.BaseUOM)
                    .Include(p => p.Generic)
                    .OrderByDescending(p => p.ProductId);

                var result = await query.ToPagedResultAsync(page, pageSize);

                if (branchId.HasValue)
                {
                    await ApplyBranchStockAsync(result.Items, branchId.Value);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading products: " + ex.Message);
            }
        }

        private IQueryable<Product> BuildActiveQuery(string? search, int? branchId)
        {
            var query = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower().Trim();
                query = query.Where(p => p.ProductName.ToLower().Contains(search) || (p.SKU != null && p.SKU.ToLower().Contains(search)));
            }

            if (branchId.HasValue)
            {
                // Only show products that have at least one stock entry in this branch, or are Services
                query = query.Where(p => _context.BranchLotStocks.Any(s => s.BranchId == branchId.Value && s.ProductId == p.ProductId && s.CurrentStock > 0) || p.IsService);
            }

            return query;
        }

        /// <summary>
        /// Overrides CurrentStock with the branch-specific total for each
        /// product in the given (already materialized) set, using a single
        /// grouped query instead of one SumAsync per product.
        /// </summary>
        private async Task ApplyBranchStockAsync(IEnumerable<Product> products, int branchId)
        {
            var ids = products.Select(p => p.ProductId).Distinct().ToList();
            if (ids.Count == 0) return;

            var sums = await _context.BranchLotStocks
                .Where(s => s.BranchId == branchId && ids.Contains(s.ProductId))
                .GroupBy(s => s.ProductId)
                .Select(g => new { ProductId = g.Key, Total = g.Sum(s => (decimal?)s.CurrentStock) ?? 0 })
                .ToDictionaryAsync(x => x.ProductId, x => x.Total);

            foreach (var product in products)
            {
                product.CurrentStock = sums.TryGetValue(product.ProductId, out var total) ? total : 0;
            }
        }

        public async Task<IEnumerable<Product>> GetDeletedListAsync()
        {
            try
            {
                return await _context.Products.IgnoreQueryFilters().Where(p => p.IsDeleted).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading deleted products: " + ex.Message);
            }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) throw new Exception("Product not found for deletion.");

                // Check for dependencies (Stocks, Sales, Purchases)
                var hasStock = await _context.BranchLotStocks.AnyAsync(s => s.ProductId == id && (s.CurrentStock > 0 || s.DamagedStock > 0));
                if (hasStock) throw new Exception("Cannot delete product because it has remaining stock in branches.");

                // Ordering guard: a product used by live documents cannot
                // be deleted — the purchase / sale / return / transfer
                // details look the product up by id, and deleting it
                // would blank those lookups. Delete the documents first.
                // (Only live documents count: the master queries below
                // are filtered to non-deleted rows, so documents already
                // in trash do not block the delete.)
                var blockers = new List<string>();
                if (await _context.PurchaseMasters.AnyAsync(p => p.PurchaseDetails.Any(d => d.ProductId == id))) blockers.Add("purchases");
                if (await _context.SalesMasters.AnyAsync(s => s.SalesDetails.Any(d => d.ProductId == id))) blockers.Add("sales");
                if (await _context.PurchaseReturnMasters.AnyAsync(r => r.ReturnDetails.Any(d => d.ProductId == id))) blockers.Add("purchase returns");
                if (await _context.SalesReturnMasters.AnyAsync(r => r.ReturnDetails.Any(d => d.ProductId == id))) blockers.Add("sales returns");
                if (await _context.TransferMasters.AnyAsync(t => t.TransferDetails.Any(d => d.ProductId == id))) blockers.Add("transfers");
                if (await _context.TransferRequisitions.AnyAsync(r => r.RequisitionDetails.Any(d => d.ProductId == id))) blockers.Add("requisitions");
                if (blockers.Count > 0)
                    throw new Exception($"Cannot delete product {product.ProductName}: it is used by {string.Join(", ", blockers)}. Delete those records first, then delete the product.");

                product.IsDeleted = true;
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
                var product = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProductId == id);
                if (product == null) throw new Exception("Deleted product not found.");
                product.IsDeleted = false;
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products.AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.UOM)
                    .Include(p => p.BaseUOM)
                    .Include(p => p.Generic)
                    .Include(p => p.UnitConversions)
                    .FirstOrDefaultAsync(x => x.ProductId == id);
                
                return product;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateAsync(Product product, string? newGenericName = null, string? newCategoryName = null, string? newBrandName = null, string? newLocationName = null, string? newUomName = null, string? userName = null)
        {
            if (product == null) throw new Exception("Product data is missing.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _context.Products.FindAsync(product.ProductId);
                if (existing == null) throw new Exception("Product not found for update.");

                var nameExists = await _context.Products.AnyAsync(p => p.ProductName.ToLower() == product.ProductName.ToLower() && p.ProductId != product.ProductId && !p.IsDeleted);
                if (nameExists) throw new Exception($"Another product with name '{product.ProductName}' already exists.");

                if (!string.IsNullOrWhiteSpace(product.SKU))
                {
                    var duplicateProduct = await _context.Products.AsNoTracking()
                        .FirstOrDefaultAsync(p => 
                            p.ProductId != product.ProductId && 
                            !p.IsDeleted && 
                            p.SKU != null && 
                            p.SKU.Trim().ToLower() == product.SKU.Trim().ToLower());

                    if (duplicateProduct != null) 
                        throw new Exception($"This SKU is already assigned to another product: '{duplicateProduct.ProductName}'.");
                }

                if (!string.IsNullOrWhiteSpace(newGenericName))
                {
                    var generic = new GenericInfo { GenericName = newGenericName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.GenericInfos.AddAsync(generic);
                    await _context.SaveChangesAsync();
                    product.GenericId = generic.GenericId;
                }
                if (!string.IsNullOrWhiteSpace(newCategoryName))
                {
                    var category = new Category { CategoryName = newCategoryName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.Categories.AddAsync(category);
                    await _context.SaveChangesAsync();
                    product.CategoryId = category.CategoryId;
                }
                if (!string.IsNullOrWhiteSpace(newBrandName))
                {
                    var brand = new Manufacturer { BrandName = newBrandName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.Manufacturers.AddAsync(brand);
                    await _context.SaveChangesAsync();
                    product.BrandId = brand.BrandId;
                }
                if (!string.IsNullOrWhiteSpace(newLocationName))
                {
                    var location = new StoreLocation { ShelfName = newLocationName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.StoreLocations.AddAsync(location);
                    await _context.SaveChangesAsync();
                    product.LocationId = location.LocationId;
                }
                if (!string.IsNullOrWhiteSpace(newUomName))
                {
                    var uom = new UOM { UOMName = newUomName.Trim(), IsActive = true, CreatedBy = userName };
                    await _context.UOMs.AddAsync(uom);
                    await _context.SaveChangesAsync();
                    product.UOMId = uom.UOMId;
                    product.BaseUOMId = uom.UOMId;
                }

                _context.Entry(existing).CurrentValues.SetValues(product);
                var success = await _context.SaveChangesAsync() > 0;
                await transaction.CommitAsync();
                return success;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
