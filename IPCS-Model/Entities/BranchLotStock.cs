using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Model.Entities
{
    // Indexing For Fast Performance
    [Index(nameof(BranchId), nameof(ProductId), nameof(LotId), Name = "IX_Branch_Product_Lot")]
    public class BranchLotStock
    {
        [Key]
        public int StockId { get; set; }

        [Required]
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        public int LotId { get; set; }
        [ForeignKey("LotId")]
        public virtual LotInfo? Lot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentStock { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal DamagedStock { get; set; } // Stock that is not sellable

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}