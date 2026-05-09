namespace IPCS_Model.DTOs
{
    public class CustomerDTO
    {
        public int CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Address { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentDue { get; set; }
        public decimal AdvanceBalance { get; set; }
        public bool IsDue { get; set; }
        public bool IsActive { get; set; } = true;
    }
}