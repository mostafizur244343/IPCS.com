namespace IPCS_Model.DTOs
{
    public class StockLedgerDTO
    {
        public int BranchId { get; set; }
        public int ProductId { get; set; }
        public int LotId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal QuantityIn { get; set; }
        public decimal QuantityOut { get; set; }
        public string? TransactionType { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Remarks { get; set; }
    }
}