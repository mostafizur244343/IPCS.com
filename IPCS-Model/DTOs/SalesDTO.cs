using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class SalesMasterDTO
    {
        public int SalesId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime SalesDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Branch is Required")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Customer is Required")]
        public int CustomerId { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public bool IsChangeConvertedToCredit { get; set; } = false;
        public bool IsChangeTakenAsIncome { get; set; } = false;
        public string PaymentStatus { get; set; } = "Paid";
        public string? Remarks { get; set; }

        public List<SalesDetailsDTO> SalesDetails { get; set; } = new List<SalesDetailsDTO>();
        public List<InvoicePaymentDTO> Payments { get; set; } = new List<InvoicePaymentDTO>();
    }

    public class SalesDetailsDTO
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int LotId { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public int UOMId { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal LineTotal { get; set; }
    }
}
