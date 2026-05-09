using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface IProductUnitConversionService
    {
        Task<IEnumerable<ProductUnitConversion>> GetByProductIdAsync(int productId);
        Task<bool> AddConversionAsync(ProductUnitConversion conversion);
        Task<bool> DeleteConversionAsync(int id);
    }
}