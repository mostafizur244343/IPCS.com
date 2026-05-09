namespace IPCS_Model.DTOs
{
    public class GlobalUnitConversionDTO
    {
        public int Id { get; set; }
        public int FromUnitId { get; set; }
        public string? FromUnitName { get; set; } // For display
        public int ToUnitId { get; set; }
        public string? ToUnitName { get; set; } // For display
        public decimal ConversionFactor { get; set; }
        public bool IsActive { get; set; }
    }
}
