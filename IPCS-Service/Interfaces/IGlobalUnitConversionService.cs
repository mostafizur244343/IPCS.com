using IPCS_Model.Entities;
using IPCS_Model.DTOs;

namespace IPCS_Service.Interfaces
{
    public interface IGlobalUnitConversionService
    {
        Task<IEnumerable<GlobalUnitConversionDTO>> GetAllAsync();
        Task<bool> CreateAsync(GlobalUnitConversion entity);
        Task<bool> DeleteAsync(int id);
    }
}
