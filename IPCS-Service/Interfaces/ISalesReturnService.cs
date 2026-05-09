using IPCS_Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface ISalesReturnService
    {
        Task<IEnumerable<SalesReturnMaster>> GetAllActiveAsync();
        Task<SalesReturnMaster?> GetByIdAsync(int id);
        Task<bool> CreateReturnAsync(SalesReturnMaster returnMaster);
        Task<bool> SoftDeleteAsync(int id);
    }
}
