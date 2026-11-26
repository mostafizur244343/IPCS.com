using IPCS_Model.Entities;
using IPCS_Model.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface IPurchaseService
    {
        Task<IEnumerable<PurchaseMaster>> GetAllActiveAsync(string? search = null);
        Task<PagedResult<PurchaseMaster>> GetPagedAsync(int page, int pageSize, string? search = null);
        Task<IEnumerable<PurchaseMaster>> GetDeletedListAsync();
        Task<PurchaseMaster?> GetByIdAsync(int id);
        Task<bool> CreatePurchaseAsync(PurchaseMaster purchaseMaster);
        Task<bool> ReceiveShipmentAsync(int purchaseId);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> UpdateHeaderAsync(int id, DocumentHeaderUpdateDTO dto);
        Task<bool> RestoreAsync(int id);
    }
}
