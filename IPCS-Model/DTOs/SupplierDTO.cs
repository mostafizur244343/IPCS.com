namespace IPCS_Model.DTOs
{
    public class SupplierDTO
    {
        public string SupplierName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public bool IsDue { get; set; }
        public bool IsActive { get; set; } = true;
    }
}