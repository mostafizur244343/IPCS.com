using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class PurchaseReturnDTO
    {
        public int ReturnId { get; set; }
        public string? ReturnNo { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        public int? PurchaseId { get; set; }
        [Required]
        public int SupplierId { get; set; }
        [Required]
        public int BranchId { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundType { get; set; } = "Adjustment"; // Cash, Adjustment
        public int? PaymentMethodId { get; set; }
        public string? Reason { get; set; }

        public List<PurchaseReturnDetailsDTO> ReturnDetails { get; set; } = new List<PurchaseReturnDetailsDTO>();
    }

    public class PurchaseReturnDetailsDTO
    {
        public int ProductId { get; set; }
        public int LotId { get; set; }
        public decimal Quantity { get; set; }
        public int UOMId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal LineTotal { get; set; }
        public bool FromDamagedPool { get; set; }
    }
}
