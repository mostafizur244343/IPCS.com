using IPCS_Model.DTOs;
using IPCS_Model.Entities;

namespace IPCS_Service.Interfaces
{
    public interface IBranchLotStockService
    {
        Task<IEnumerable<object>> GetAllAsync(); // Using AsNoTracking 
        Task<object?> GetStockDetailAsync(int branchId, int productId);
        Task<IEnumerable<object>> GetLowStockAlertAsync(int branchId);
        Task<bool> AdjustStockAsync(BranchLotStockDTO model); // Stock Adjustment
        Task<bool> UpdateStockInternalAsync(int branchId, int productId, int lotId, decimal quantity, bool isAddition);
        Task<IEnumerable<object>> GetActiveLotsByProductAndBranchAsync(int productId, int branchId);
    }
}