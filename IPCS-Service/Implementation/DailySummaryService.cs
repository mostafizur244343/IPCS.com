using IPCS_Model.Entities;
using IPCS_Model.DTOs;
using IPCS_Service.Interfaces;
using IPCS_Repo.Data;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Service.Implementation
{
    public class DailySummaryService : IDailySummaryService
    {
        private readonly IPCSDBContext _context;
        public DailySummaryService(IPCSDBContext context) { _context = context; }

        public async Task<bool> UpsertDailySummaryAsync(DailySummaryDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var dateOnly = dto.TransactionDate.Date;
                var existing = await _context.DailyTransactionSummaries
                    .FirstOrDefaultAsync(x => x.BranchId == dto.BranchId && x.TransactionDate == dateOnly);

                if (existing == null)
                {
                    var newSummary = new DailyTransactionSummary
                    {
                        BranchId = dto.BranchId,
                        TransactionDate = dateOnly,
                        OpeningBalance = dto.OpeningBalance,
                        TotalCashSales = dto.TotalCashSales,
                        TotalDueSales = dto.TotalDueSales,
                        TotalCollection = dto.TotalCollection,
                        TotalPurchase = dto.TotalPurchase,
                        TotalExpense = dto.TotalExpense,
                        TotalDiscount = dto.TotalDiscount,
                        NetProfit = dto.NetProfit,
                        Remarks = dto.Remarks
                    };
                    await _context.DailyTransactionSummaries.AddAsync(newSummary);
                }
                else
                {
                    existing.OpeningBalance = dto.OpeningBalance;
                    existing.TotalCashSales = dto.TotalCashSales;
                    existing.TotalDueSales = dto.TotalDueSales;
                    existing.TotalCollection = dto.TotalCollection;
                    existing.TotalPurchase = dto.TotalPurchase;
                    existing.TotalExpense = dto.TotalExpense;
                    existing.TotalDiscount = dto.TotalDiscount;
                    existing.NetProfit = dto.NetProfit;
                    existing.Remarks = dto.Remarks;

                    _context.DailyTransactionSummaries.Update(existing);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch { await transaction.RollbackAsync(); throw; }
        }

        // Monthly report logic
        public async Task<object> GetMonthlyReportAsync(int branchId, int year, int month)
        {
            // Starting and ending date of that month
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var report = await _context.DailyTransactionSummaries
                .AsNoTracking()
                .Where(x => x.BranchId == branchId &&
                            x.TransactionDate >= startDate &&
                            x.TransactionDate <= endDate)
                .GroupBy(x => x.BranchId)
                .Select(g => new {
                    Month = month,
                    Year = year,
                    TotalSales = g.Sum(x => x.TotalCashSales + x.TotalDueSales),
                    TotalNetProfit = g.Sum(x => x.NetProfit),
                    TotalCollection = g.Sum(x => x.TotalCollection),
                    AvgClosingCash = g.Average(x => x.ClosingCash)
                })
                .FirstOrDefaultAsync();

            if (report == null)
            {
                return new { Month = month, TotalSales = 0m, TotalNetProfit = 0m, AvgClosingCash = 0m };
            }

            return report;
        }

        public async Task<IEnumerable<DailyTransactionSummary>> GetAllAsync(int branchId) =>
            await _context.DailyTransactionSummaries.AsNoTracking().Where(b => b.BranchId == branchId).ToListAsync();

        public async Task<DailyTransactionSummary?> GetByDateAsync(int branchId, DateTime date) =>
            await _context.DailyTransactionSummaries.AsNoTracking().FirstOrDefaultAsync(x => x.BranchId == branchId && x.TransactionDate == date.Date);

        public async Task<object> GetYearlyReportAsync(int branchId, int year) => throw new NotImplementedException();
        public async Task<bool> DeleteAsync(long id) => throw new NotImplementedException();

        public async Task<object> GetInventoryStatsAsync()
        {
            var totalItems = await _context.Products.CountAsync(p => !p.IsDeleted);
            var lowStock = await _context.Products.CountAsync(p => !p.IsDeleted && p.CurrentStock <= p.ReorderLevel && p.CurrentStock > 0);
            var outOfStock = await _context.Products.CountAsync(p => !p.IsDeleted && p.CurrentStock <= 0);

            return new
            {
                TotalItems = totalItems,
                LowStock = lowStock,
                OutOfStock = outOfStock
            };
        }
    }
}