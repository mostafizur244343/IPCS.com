using System.Collections.Generic;

namespace IPCS_Model.DTOs
{
    /// <summary>
    /// Lightweight dashboard payload: today's sales total for the requested
    /// day plus the 5 most recent sales. Replaces the dashboard's old habit
    /// of downloading the entire Sales table to compute these two numbers.
    /// </summary>
    public class SalesDashboardSummaryDTO
    {
        public decimal TodaySalesTotal { get; set; }
        public int TodaySalesCount { get; set; }
        public List<SalesListDTO> RecentSales { get; set; } = new List<SalesListDTO>();
    }
}
