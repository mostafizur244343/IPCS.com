using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllActiveAsync();
        Task<IEnumerable<Product>> GetDeletedListAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Product product, decimal openingQty, decimal openingCost, int selectedUnitId, string? newGenericName = null, string? newCategoryName = null, string? newBrandName = null, string? newLocationName = null, string? newUomName = null, string? userName = null);
        Task<decimal> ConvertToBaseUnit(int productId, int selectedUnitId, decimal quantity);
        Task<decimal> ConvertFromBaseUnit(int productId, int targetUnitId, decimal quantity);
        Task<bool> UpdateAsync(Product product, string? newGenericName = null, string? newCategoryName = null, string? newBrandName = null, string? newLocationName = null, string? newUomName = null, string? userName = null);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<string> GenerateProductCodeAsync();
        // Moving Average Cost Calculation Method
        Task UpdateMovingAverageCostAsync(int productId, decimal newQty, decimal newUnitPrice);
    }
}