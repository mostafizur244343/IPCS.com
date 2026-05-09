using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [StringLength(20)]
        public string? CustomerCode { get; set; } // Auto Generated

        [Required(ErrorMessage = "Must Input Customer Name")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Must Input Mobile Number")]
        [StringLength(15)]
        [RegularExpression(@"^(?:\+88|88)?(01[3-9]\d{8})$", ErrorMessage = "Enter Valid BD Mobile Number")]

        public string Mobile { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; } // _ Due and - Advance

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentDue { get; set; } // Calculatable with Payment

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceBalance { get; set; } // Wallet/Credit Balance from overpayment

        public bool IsDue { get; set; } // True = Due, False = For Advance Opening

        public bool IsDeleted { get; set; } = false; // Soft Delete
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public virtual ICollection<SalesMaster> SalesMasters { get; set; } = new List<SalesMaster>();
    }
}