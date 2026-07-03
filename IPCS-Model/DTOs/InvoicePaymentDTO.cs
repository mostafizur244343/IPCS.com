using System;
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class InvoicePaymentDTO
    {
        public int PaymentId { get; set; }
        public string TransactionType { get; set; } = "Sale";
        public int? SaleId { get; set; }
        public int? PurchaseId { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string? TransactionNo { get; set; }
        public string? Remarks { get; set; }
    }
}
