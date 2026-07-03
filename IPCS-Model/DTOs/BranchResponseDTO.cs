using System.Collections.Generic;

namespace IPCS_Model.DTOs
{
    public class BranchResponseDTO : BranchDTO
    {
        public int BranchId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        
        // Reporting fields
        public int TotalSentTransfers { get; set; }
        public int TotalReceivedTransfers { get; set; }
        public int TotalSentRequisitions { get; set; }
        public int TotalReceivedRequisitions { get; set; }
        public int TotalSales { get; set; }
        public int TotalPurchases { get; set; }
    }
}
