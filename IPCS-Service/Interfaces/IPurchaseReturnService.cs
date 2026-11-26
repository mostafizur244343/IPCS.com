using IPCS_Model.Entities;
using IPCS_Model.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface IPurchaseReturnService
    {
        Task<IEnumerable<PurchaseReturnMaster>> GetAllActiveAsync(string? search = null);
        Task<PagedResult<PurchaseReturnMaster>> GetPagedAsync(int page, int pageSize, string? search = null);
        Task<PurchaseReturnMaster?> GetByIdAsync(int id);
        Task<bool> CreateReturnAsync(PurchaseReturnMaster returnMaster);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<IEnumerable<PurchaseReturnMaster>> GetDeletedListAsync();
        Task<bool> UpdateHeaderAsync(int id, DocumentHeaderUpdateDTO dto);
    }
}
