using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface ILotInfoService
    {
        Task<IEnumerable<LotInfo>> GetAllAsync();
        Task<LotInfo?> GetByIdAsync(int id);
        Task<bool> CreateAsync(LotInfo lotInfo);
        Task<bool> UpdateAsync(LotInfo lotInfo);
        Task<bool> DeleteAsync(int id);
    }
}