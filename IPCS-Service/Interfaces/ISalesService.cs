using IPCS_Model.Entities;
using IPCS_Model.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPCS_Service.Interfaces
{
    public interface ISalesService
    {
        Task<IEnumerable<SalesMaster>> GetAllActiveAsync(string? search = null);
        Task<PagedResult<SalesListDTO>> GetPagedListAsync(int page, int pageSize, string? search = null);
        Task<SalesDashboardSummaryDTO> GetDashboardSummaryAsync(DateTime? from, DateTime? to);
        Task<IEnumerable<SalesMaster>> GetDeletedListAsync();
        Task<SalesMaster?> GetByIdAsync(int id);
        Task<bool> CreateSalesAsync(SalesMaster salesMaster);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> UpdateHeaderAsync(int id, DocumentHeaderUpdateDTO dto);
        Task<bool> RestoreAsync(int id);
    }
}
