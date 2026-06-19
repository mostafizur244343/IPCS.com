using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Model.Entities
{
    //  Configuring Index On DBContext but here just Attributes
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [StringLength(20)]
        public string? SupplierCode { get; set; } // Auto Genereted

        [Required(ErrorMessage = "Must Input Supplier Name")]
        [StringLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string Mobile { get; set; } = string.Empty;

        public string? PicturePath { get; set; } // Supplier profile picture

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; } // Be + Or -

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentDue { get; set; } // Support + and - Both Value

        public bool IsDue { get; set; } // True = Due, False = Advance (Just for Openning)

        public bool IsDeleted { get; set; } = false; // Soft Delete Field
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}