namespace IPCS_Model.DTOs
{
    public class LotInfoDTO
    {
        public int ProductId { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public DateTime? ManufacturingDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public bool IsActive { get; set; } = true;
    }
}