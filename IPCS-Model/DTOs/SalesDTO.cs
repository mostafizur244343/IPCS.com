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

        public int? CustomerId { get; set; }

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
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int LotId { get; set; }
        public decimal Quantity { get; set; }
        public int? UOMId { get; set; } 
        public string? UomName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class SalesListDTO
    {
        public int SalesId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime SalesDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string PaymentStatus { get; set; } = "Paid";
    }
}
