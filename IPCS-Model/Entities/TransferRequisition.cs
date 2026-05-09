using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class TransferRequisition
    {
        [Key]
        public int RequisitionId { get; set; }

        [StringLength(20)]
        public string? RequisitionCode { get; set; } // REQ-00001
        
        [Required]
        public int FromBranchId { get; set; } // The branch requesting the products
        [ForeignKey("FromBranchId")]
        [InverseProperty("SentRequisitions")]
        public virtual Branch? FromBranch { get; set; }

        [Required]
        public int ToBranchId { get; set; } // The branch to fulfill the request (Usually Main Branch)
        [ForeignKey("ToBranchId")]
        [InverseProperty("ReceivedRequisitions")]
        public virtual Branch? ToBranch { get; set; }

        public DateTime RequisitionDate { get; set; } = DateTime.Now;
        public DateTime ExpectedDate { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Partially Transferred, Transferred, Rejected

        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<TransferRequisitionDetails> RequisitionDetails { get; set; } = new List<TransferRequisitionDetails>();
    }

    public class TransferRequisitionDetails
    {
        [Key]
        public int RequisitionDetailId { get; set; }

        public int RequisitionId { get; set; }
        [ForeignKey("RequisitionId")]
        public virtual TransferRequisition? TransferRequisition { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RequestQty { get; set; }

        public int UOMId { get; set; }
        [ForeignKey("UOMId")]
        public virtual UOM? UOM { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RequestQtyInPcs { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ApprovedQtyInPcs { get; set; } = 0;
    }
}
