using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class InvoicePayment
    {
        [Key]
        public int PaymentId { get; set; }

        // Transaction Type: "Sale", "Purchase", "CustomerReceive", "SupplierPayment"
        [Required]
        public string TransactionType { get; set; } = string.Empty;

        // Foreign Keys for Sales (Nullable)
        public int? SaleId { get; set; }
        [ForeignKey("SaleId")]
        public virtual SalesMaster? Sale { get; set; }

        // Foreign Keys for Purchase (Nullable)
        public int? PurchaseId { get; set; }
        [ForeignKey("PurchaseId")]
        public virtual PurchaseMaster? Purchase { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? TransactionNo { get; set; } // Bkash/Nagad Transaction ID

        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }

        // True when this payment was recorded through POST /api/Payment/
        // process-split (a later settlement of an invoice's due). Inline
        // payments created together with a sale/purchase, and the REF- /
        // PREF- audit payments created by returns, are false. The flag is
        // the only reliable way to tell a split settlement apart from an
        // inline payment: it decides which payments a sale delete/restore
        // counts towards TotalCollection and TotalCashSales (a split
        // settlement went to TotalCollection, an inline payment went to
        // TotalCashSales). See GUARD_RELAX.md.
        public bool IsSplitSettlement { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
    }
}
