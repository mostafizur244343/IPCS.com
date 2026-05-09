using IPCS_Model.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class PurchaseDetails
    {
        [Key]
        public int PurchaseDetailId { get; set; }

        public int PurchaseId { get; set; }
        [ForeignKey("PurchaseId")] public virtual PurchaseMaster? PurchaseMaster { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")] public virtual Product? Product { get; set; }

        [StringLength(50)]
        public string BatchNo { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal FreeQty { get; set; }

        public int UOMId { get; set; }
        [ForeignKey("UOMId")] public virtual UOM? UOM { get; set; }

        public bool IsInBox { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ConversionFactor { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } // Box/Strip/Pcs Price

        [Column(TypeName = "decimal(18,2)")]
        public decimal MRP { get; set; } // Current Market Price

        [NotMapped]
        public decimal TotalQtyInPcs => IsInBox ? ((Qty + FreeQty) * ConversionFactor) : (Qty + FreeQty);

        [NotMapped]
        public decimal UnitCostInPcs => IsInBox ? (PurchasePrice / ConversionFactor) : PurchasePrice;

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; } // Qty * PurchasePrice
    }
}