using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllActiveAsync(); // IsDeleted = false
        Task<IEnumerable<Supplier>> GetDeletedListAsync(); // Deleted List For Dashboard
        Task<Supplier?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Supplier supplier);
        Task<bool> UpdateAsync(Supplier supplier);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id); // For Return deleted Supplier
        Task<string> GenerateSupplierCodeAsync();
    }
}