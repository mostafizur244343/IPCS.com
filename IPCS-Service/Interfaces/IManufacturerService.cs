
using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface IManufacturerService
    {
        Task<IEnumerable<Manufacturer>> GetAllAsync();
        Task<Manufacturer?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Manufacturer manufacturer);
        Task<bool> UpdateAsync(Manufacturer manufacturer);
        Task<bool> DeleteAsync(int id);
    }
}
