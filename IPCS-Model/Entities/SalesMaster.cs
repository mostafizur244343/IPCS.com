using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class SalesMaster
    {
        [Key]
        public int SalesId { get; set; }

        [Required]
        [StringLength(20)]
        public string InvoiceNo { get; set; } = string.Empty;

        [Required]
        public DateTime SalesDate { get; set; } = DateTime.Now;

        // Relation with Branch
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        // Relation with Customer
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Sum of Line Totals

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; } // Total - Discount

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DueAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ChangeAmount { get; set; }

        // New Logic Fields
        public bool IsChangeConvertedToCredit { get; set; } = false; // For Customer Wallet
        public bool IsChangeTakenAsIncome { get; set; } = false; // For Main Cash/Income

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Paid"; // Paid, Partial, Due

        public string? Remarks { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        // Master-Detail Relationship
        public virtual ICollection<SalesDetails> SalesDetails { get; set; } = new List<SalesDetails>();
        
        // Multi-Payment Relationship
        public virtual ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
    }

    public class SalesDetails
    {
        [Key]
        public int SalesDetailId { get; set; }

        public int SalesId { get; set; }
        [ForeignKey("SalesId")]
        public virtual SalesMaster? SalesMaster { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int LotId { get; set; }
        [ForeignKey("LotId")]
        public virtual LotInfo? Lot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        public int UOMId { get; set; } // The Unit used for selling (Pcs/Strip etc)
        [ForeignKey("UOMId")]
        public virtual UOM? UOM { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPerUnit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; } // (UnitPrice - DiscountPerUnit) * Quantity

        // For Profit calculation
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPriceAtSale { get; set; } // Snapshot of cost at that time
    }
}
