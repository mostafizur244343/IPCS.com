using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class SalesReturnDTO
    {
        public int ReturnId { get; set; }
        public string? ReturnNo { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        public int? SalesId { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int BranchId { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundType { get; set; } = "Adjustment"; // Cash, Adjustment, Wallet, Credit
        public int? PaymentMethodId { get; set; } // Which method used for refund?
        public string? Reason { get; set; }

        public List<SalesReturnDetailsDTO> ReturnDetails { get; set; } = new List<SalesReturnDetailsDTO>();
    }

    public class SalesReturnDetailsDTO
    {
        public int ProductId { get; set; }
        public int LotId { get; set; }
        public decimal Quantity { get; set; }
        public int UOMId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
