namespace IPCS_Model.DTOs
{
    public class PaymentMethodDTO
    {
        public string MethodName { get; set; } = string.Empty;
        public bool IsDigital { get; set; }
        public decimal ExtraChargePercentage { get; set; }
        public string? AccountNumber { get; set; }
        public string? IconPath { get; set; }
        public string? QRCodePath { get; set; }
        public decimal MinimumAmount { get; set; }
        public bool IsActive { get; set; } = true;
    }
}