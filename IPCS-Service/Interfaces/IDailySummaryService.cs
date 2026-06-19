using IPCS_Model.Entities;
using IPCS_Model.DTOs;

namespace IPCS_Service.Interfaces
{
    public interface IDailySummaryService
    {
        Task<bool> UpsertDailySummaryAsync(DailySummaryDTO dto); // Today Creating or Updating
        Task<IEnumerable<DailyTransactionSummary>> GetAllAsync(int branchId);
        Task<DailyTransactionSummary?> GetByDateAsync(int branchId, DateTime date);
        Task<object> GetMonthlyReportAsync(int branchId, int year, int month); // Monthly Report
        Task<object> GetYearlyReportAsync(int branchId, int year); // Yearly Report
        Task<bool> DeleteAsync(long id);
        Task<object> GetInventoryStatsAsync();
    }
}