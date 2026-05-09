using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface IGenericInfoService
    {
        Task<IEnumerable<GenericInfo>> GetAllAsync();
        Task<GenericInfo?> GetByIdAsync(int id);
        Task<bool> CreateAsync(GenericInfo genericInfo);
        Task<bool> UpdateAsync(GenericInfo genericInfo);
        Task<bool> DeleteAsync(int id);
    }
}