using IPCS_Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface IPurchaseService
    {
        Task<IEnumerable<PurchaseMaster>> GetAllActiveAsync();
        Task<IEnumerable<PurchaseMaster>> GetDeletedListAsync();
        Task<PurchaseMaster?> GetByIdAsync(int id);
        Task<bool> CreatePurchaseAsync(PurchaseMaster purchaseMaster);
        Task<bool> ReceiveShipmentAsync(int purchaseId);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}
