namespace IPCS_Model.DTOs
{
    public class DailySummaryDTO
    {
        public int BranchId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalCashSales { get; set; }
        public decimal TotalDueSales { get; set; }
        public decimal TotalCollection { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetProfit { get; set; }
        public string? Remarks { get; set; }
    }
}