using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.Entities
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Must input Brunch Name")]
        public string BranchName { get; set; } = string.Empty;

        public string? BranchCode { get; set; } // Auto Generated

        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? ManagerName { get; set; }
        public string? PicturePath { get; set; } // Branch image

        public bool IsActive { get; set; } = true;

        public virtual ICollection<TransferMaster> SentTransfers { get; set; } = new List<TransferMaster>();
        public virtual ICollection<TransferMaster> ReceivedTransfers { get; set; } = new List<TransferMaster>();
        public virtual ICollection<TransferRequisition> SentRequisitions { get; set; } = new List<TransferRequisition>();
        public virtual ICollection<TransferRequisition> ReceivedRequisitions { get; set; } = new List<TransferRequisition>();
        public virtual ICollection<SalesMaster> SalesMasters { get; set; } = new List<SalesMaster>();
        public virtual ICollection<PurchaseMaster> PurchaseMasters { get; set; } = new List<PurchaseMaster>();

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}