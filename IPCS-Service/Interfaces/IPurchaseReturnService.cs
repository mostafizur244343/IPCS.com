using IPCS_Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface IPurchaseReturnService
    {
        Task<IEnumerable<PurchaseReturnMaster>> GetAllActiveAsync();
        Task<PurchaseReturnMaster?> GetByIdAsync(int id);
        Task<bool> CreateReturnAsync(PurchaseReturnMaster returnMaster);
        Task<bool> SoftDeleteAsync(int id);
    }
}
