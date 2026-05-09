using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class TransferMaster
    {
        [Key]
        public int TransferId { get; set; }

        [StringLength(20)]
        public string? TransferCode { get; set; } // TRN-00001
        
        public int? RequisitionId { get; set; } // Nullable, as transfer can be direct
        [ForeignKey("RequisitionId")]
        public virtual TransferRequisition? Requisition { get; set; }

        [Required]
        public int FromBranchId { get; set; } // The branch sending products (Usually Main Branch)
        [ForeignKey("FromBranchId")]
        [InverseProperty("SentTransfers")]
        public virtual Branch? FromBranch { get; set; }

        [Required]
        public int ToBranchId { get; set; } // The branch receiving products
        [ForeignKey("ToBranchId")]
        [InverseProperty("ReceivedTransfers")]
        public virtual Branch? ToBranch { get; set; }

        public DateTime TransferDate { get; set; } = DateTime.Now;

        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Received, Canceled, CancelConfirmed

        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string? ReceivedBy { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCharge { get; set; } = 0; // Transportation cost

        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<TransferDetails> TransferDetails { get; set; } = new List<TransferDetails>();
    }

    public class TransferDetails
    {
        [Key]
        public int TransferDetailId { get; set; }

        public int TransferId { get; set; }
        [ForeignKey("TransferId")]
        public virtual TransferMaster? TransferMaster { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int UOMId { get; set; }
        [ForeignKey("UOMId")]
        public virtual UOM? UOM { get; set; }

        public int LotId { get; set; } // Specific Lot being transferred
        [ForeignKey("LotId")]
        public virtual LotInfo? Lot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TransferQty { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal TransferQtyInPcs { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReceivedQty { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReceivedQtyInPcs { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal DamageQty { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal DamageQtyInPcs { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Selling Price during transfer

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; } // Qty * CostPrice (or UnitPrice) balance tracking

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; } // Purchase Price from Lot
    }
}
