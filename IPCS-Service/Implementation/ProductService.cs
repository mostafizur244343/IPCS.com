using IPCS_Model.Entities;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class ProductService : 
        IProductService
    {
        private readonly IPCSDBContext _context;
        public ProductService(IPCSDBContext context) { _context = context; }

        public async Task<string> GenerateProductCodeAsync()
        {
            var last = await _context.Products.IgnoreQueryFilters().AsNoTracking().OrderByDescending(p => p.ProductId).FirstOrDefaultAsync();
            int nextId = (last == null) ? 1 : last.ProductId + 1;
            return $"PROD-{nextId:D5}";
        }



        // Multi Conversion Logic
        public async Task<decimal> ConvertToBaseUnit(int productId, int selectedUnitId, decimal quantity)
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null) return quantity;

            // If User Enter Dirctly in BaseUnit then no conversion need
            if (product.BaseUOMId == selectedUnitId) return quantity;

            // Get back All Conversion Rules From Database 
            var conversions = await _context.ProductUnitConversions
                                  .AsNoTracking()
                                  .Where(p => p.ProductId == productId)
                                  .OrderBy(p => p.Level)
                                  .ToListAsync();

            decimal finalQty = quantity;
            bool foundStart = false;

            foreach (var conv in conversions)
            {
                // user start which unit start convert from there to under
                if (conv.FromUnitId == selectedUnitId || foundStart)
                {
                    finalQty *= conv.Factor;
                    foundStart = true;
                }
            }
            return finalQty;
        }

        // Logic: Opening Stock = Current Stock & Opening Cost = Cost Price in the New Creating Time
        public async Task<bool> CreateAsync(Product product, decimal openingQty, decimal openingCost , int selectedUnitId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                product.ProductCode = await GenerateProductCodeAsync();


                // first save the logic for get id
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                // Call Conversion logic
                decimal baseQty = await ConvertToBaseUnit(product.ProductId, selectedUnitId, openingQty);

                // If it's a Service, we skip stock and cost calculation logic
                if (product.IsService)
                {
                    product.CurrentStock = 0;
                    product.CostPrice = 0;
                }
                else
                {
                    //Find out per puc cost
                    decimal totalOpeningCost = openingQty * openingCost;

                    // *** Conversion Logic ***
                    // If User Purchase in Box It Convert to Pices
                    product.CurrentStock = baseQty;
                    product.CostPrice = (baseQty > 0) ? totalOpeningCost / baseQty : openingCost;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch { await transaction.RollbackAsync(); throw; }
        }

        // Logic: Moving Average Cost Formula
        // New Cost = ((Old Qty * Old Cost) + (New Qty * New Cost)) / (Old Qty + New Qty)
        public async Task UpdateMovingAverageCostAsync(int productId, decimal newQtyInPcs, decimal newUnitPriceInPcs)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                decimal totalValue = (product.CurrentStock * product.CostPrice) + (newQtyInPcs * newUnitPriceInPcs);
                decimal totalQty = product.CurrentStock + newQtyInPcs;

                if (totalQty > 0)
                    product.CostPrice = Math.Round(totalValue / totalQty, 2);

                product.CurrentStock = totalQty; // Global Stock Async
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Product>> GetAllActiveAsync() =>
            await _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Brand).Include(p => p.UOM).Include(p => p.UnitConversions).ToListAsync();


        public async Task<IEnumerable<Product>> GetDeletedListAsync() =>
            await _context.Products.IgnoreQueryFilters().Where(p => p.IsDeleted).AsNoTracking().ToListAsync();

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;
            product.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var product = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return false;
            product.IsDeleted = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Product?> GetByIdAsync(int id) => await _context.Products.AsNoTracking().Include(p => p.UnitConversions).FirstOrDefaultAsync(x => x.ProductId == id);

        public async Task<bool> UpdateAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}