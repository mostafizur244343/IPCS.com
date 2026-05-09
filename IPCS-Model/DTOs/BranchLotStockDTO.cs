namespace IPCS_Model.DTOs
{
    public class BranchLotStockDTO
    {
        public int BranchId { get; set; }
        public int ProductId { get; set; }
        public int LotId { get; set; }
        public decimal Quantity { get; set; } // Current sellable stock
        public decimal DamagedQuantity { get; set; } // Non-sellable stock
    }
}