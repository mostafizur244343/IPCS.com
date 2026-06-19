using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class SupplierDTO
    {
        [Required(ErrorMessage = "Supplier Name is required")]
        public string SupplierName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        public string Mobile { get; set; } = string.Empty;

        public string? PicturePath { get; set; }

        public decimal OpeningBalance { get; set; } = 0;
        
        public bool IsDue { get; set; } = true;
        
        public bool IsActive { get; set; } = true;
    }
}