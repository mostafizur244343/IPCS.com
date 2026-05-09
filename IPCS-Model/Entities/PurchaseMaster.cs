using IPCS_Model.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class PurchaseMaster
    {
        [Key]
        public int PurchaseId { get; set; }

        [StringLength(20)]
        public string? PurchaseCode { get; set; } // Auto Generated PUR-00001

        [Required(ErrorMessage = "Invoice No is Required")]
        [StringLength(50)]
        public string SupplierInvoiceNo { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        // Foreign Keys
        public int BranchId { get; set; } 
        [ForeignKey("BranchId")] public virtual Branch? Branch { get; set; }

        public int SupplierId { get; set; }
        [ForeignKey("SupplierId")] public virtual Supplier? Supplier { get; set; }

        public int MethodId { get; set; }
        [ForeignKey("MethodId")] public virtual PaymentMethod? PaymentMethod { get; set; }

        // Amounts
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DueAmount { get; set; }

        [StringLength(20)]
        public string PaymentStatus { get; set; } = string.Empty; // Paid, Due, Partial

        // Shipment specific fields
        public bool IsShipment { get; set; } = false; // Is this a shipment?
        [StringLength(20)]
        public string ShipmentStatus { get; set; } = string.Empty; // Received, Pending

        public bool IsDeleted { get; set; } = false; // Soft Delete
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }

        // M-D relationship
        public virtual ICollection<PurchaseDetails> PurchaseDetails { get; set; } = new List<PurchaseDetails>();
        public virtual ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
    }
}