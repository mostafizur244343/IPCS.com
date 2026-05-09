using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class PurchaseMasterDTO
    {
        public int PurchaseId { get; set; }
        public string? PurchaseCode { get; set; }
        
        [Required(ErrorMessage = "Invoice No is Required")]
        public string SupplierInvoiceNo { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Branch is Required")]
        public int BranchId { get; set; } 

        [Required(ErrorMessage = "Supplier is Required")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Payment Method is Required")]
        public int MethodId { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        
        public string PaymentStatus { get; set; } = string.Empty;

        public bool IsShipment { get; set; } = false;
        public string ShipmentStatus { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public List<PurchaseDetailsDTO> PurchaseDetails { get; set; } = new List<PurchaseDetailsDTO>();
        public List<InvoicePaymentDTO> Payments { get; set; } = new List<InvoicePaymentDTO>();
    }

    public class PurchaseDetailsDTO
    {
        public int ProductId { get; set; }
        public string BatchNo { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }

        public decimal Qty { get; set; }
        public decimal FreeQty { get; set; }
        public int UOMId { get; set; }
        public bool IsInBox { get; set; }
        public decimal ConversionFactor { get; set; }
        public decimal PurchasePrice { get; set; } 
        public decimal MRP { get; set; } 
        public decimal LineTotal { get; set; }
    }
}
