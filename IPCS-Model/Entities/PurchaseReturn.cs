using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class PurchaseReturnMaster
    {
        [Key]
        public int ReturnId { get; set; }

        [Required]
        [StringLength(20)]
        public string ReturnNo { get; set; } = string.Empty;

        public DateTime ReturnDate { get; set; } = DateTime.Now;

        // Relation with Purchase (Optional - can return without specific invoice)
        public int? PurchaseId { get; set; }
        [ForeignKey("PurchaseId")]
        public virtual PurchaseMaster? Purchase { get; set; }

        [Required]
        public int SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        [Required]
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; } // Cash received back or Due deducted

        [Required]
        [StringLength(20)]
        public string RefundType { get; set; } = "Adjustment"; // Cash, Adjustment

        public int? PaymentMethodId { get; set; }
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }

        public string? Reason { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public virtual ICollection<PurchaseReturnDetails> ReturnDetails { get; set; } = new List<PurchaseReturnDetails>();
    }

    public class PurchaseReturnDetails
    {
        [Key]
        public int ReturnDetailId { get; set; }

        public int ReturnId { get; set; }
        [ForeignKey("ReturnId")]
        public virtual PurchaseReturnMaster? ReturnMaster { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int LotId { get; set; }
        [ForeignKey("LotId")]
        public virtual LotInfo? Lot { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        public int UOMId { get; set; }
        [ForeignKey("UOMId")]
        public virtual UOM? UOM { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        // Logic Flag: Was this returned from Damaged Stock pool?
        public bool FromDamagedPool { get; set; } = false;
    }
}
