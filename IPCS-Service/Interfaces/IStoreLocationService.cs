using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface IStoreLocationService
    {
        Task<IEnumerable<StoreLocation>> GetAllAsync();
        Task<StoreLocation?> GetByIdAsync(int id);
        Task<bool> CreateAsync(StoreLocation location);
        Task<bool> UpdateAsync(StoreLocation location);
        Task<bool> DeleteAsync(int id);
    }
}