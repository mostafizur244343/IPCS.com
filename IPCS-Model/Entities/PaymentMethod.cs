using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class PaymentMethod
    {
        [Key]
        public int MethodId { get; set; }

        [Required(ErrorMessage = "Must Input Method Name")]
        public string MethodName { get; set; } = string.Empty; // Cash, Bkash, Card, Nagad

        public bool IsDigital { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraChargePercentage { get; set; } = 0;

        public string? AccountNumber { get; set; } // Bkash Or Card Numer
        public string? IconPath { get; set; } // Image Path of LOGO
        public string? QRCodePath { get; set; } // Image Path of QR Code

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumAmount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}