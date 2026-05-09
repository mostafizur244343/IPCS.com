using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class SalesReturnMaster
    {
        [Key]
        public int ReturnId { get; set; }

        [Required]
        [StringLength(20)]
        public string ReturnNo { get; set; } = string.Empty;

        public DateTime ReturnDate { get; set; } = DateTime.Now;

        // Relation with Sales (Optional)
        public int? SalesId { get; set; }
        [ForeignKey("SalesId")]
        public virtual SalesMaster? SalesMaster { get; set; }

        [Required]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [Required]
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

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

        public virtual ICollection<SalesReturnDetails> ReturnDetails { get; set; } = new List<SalesReturnDetails>();
    }

    public class SalesReturnDetails
    {
        [Key]
        public int ReturnDetailId { get; set; }

        public int ReturnId { get; set; }
        [ForeignKey("ReturnId")]
        public virtual SalesReturnMaster? ReturnMaster { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int LotId { get; set; }
        [ForeignKey("LotId")]
        public virtual LotInfo? Lot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        public int UOMId { get; set; }
        [ForeignKey("UOMId")]
        public virtual UOM? UOM { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Selling Price at return

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }
    }
}
