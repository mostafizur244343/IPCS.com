using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class LotInfo
    {
        [Key]
        public int LotId { get; set; }

        [Required]
        public int ProductId { get; set; } // Lot number of absolute Product
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        [StringLength(50)]
        public string LotNumber { get; set; } = string.Empty;

        public DateTime? ManufacturingDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } // Of this lot

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}