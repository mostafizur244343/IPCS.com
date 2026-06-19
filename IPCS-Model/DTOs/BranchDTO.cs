using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class BranchDTO
    {
        [Required(ErrorMessage = "Must input Branch Name")]
        public string BranchName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? ManagerName { get; set; }
        public string? PicturePath { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class BranchResponseDTO : BranchDTO
    {
        public int BranchId { get; set; }
        public string? BranchCode { get; set; }
        public int TotalSentTransfers { get; set; }
        public int TotalReceivedTransfers { get; set; }
        public int TotalSentRequisitions { get; set; }
        public int TotalReceivedRequisitions { get; set; }
        public int TotalSales { get; set; }
        public int TotalPurchases { get; set; }
    }
}