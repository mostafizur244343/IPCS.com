using IPCS_Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface ISalesService
    {
        Task<IEnumerable<SalesMaster>> GetAllActiveAsync();
        Task<IEnumerable<SalesMaster>> GetDeletedListAsync();
        Task<SalesMaster?> GetByIdAsync(int id);
        Task<bool> CreateSalesAsync(SalesMaster salesMaster);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}
