using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class TransferRequisitionDTO
    {
        public int RequisitionId { get; set; }
        public string? RequisitionCode { get; set; }
        [Required]
        public int FromBranchId { get; set; }
        [Required]
        public int ToBranchId { get; set; }
        public DateTime ExpectedDate { get; set; }
        public string? Remarks { get; set; }
        public List<TransferRequisitionDetailsDTO> Details { get; set; } = new List<TransferRequisitionDetailsDTO>();
    }

    public class TransferRequisitionDetailsDTO
    {
        public int ProductId { get; set; }
        public decimal RequestQty { get; set; }
        public int UOMId { get; set; }
    }

    public class TransferMasterDTO
    {
        public int TransferId { get; set; }
        public string? TransferCode { get; set; }
        public int? RequisitionId { get; set; }
        [Required]
        public int FromBranchId { get; set; }
        [Required]
        public int ToBranchId { get; set; }
        public DateTime TransferDate { get; set; } = DateTime.Now;
        public DateTime? ReceivedDate { get; set; }
        public string? ReceivedBy { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Remarks { get; set; }
        public decimal ShippingCharge { get; set; }
        public List<TransferDetailsDTO> Details { get; set; } = new List<TransferDetailsDTO>();
    }

    public class TransferDetailsDTO
    {
        public int TransferDetailId { get; set; }
        public int ProductId { get; set; }
        public int UOMId { get; set; }
        public int LotId { get; set; }
        public decimal TransferQty { get; set; }
        public decimal TransferQtyInPcs { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal ReceivedQtyInPcs { get; set; }
        public decimal DamageQty { get; set; }
        public decimal DamageQtyInPcs { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal CostPrice { get; set; }
    }

    public class TransferReceiveDTO
    {
        public int TransferId { get; set; }
        public List<TransferReceiveDetailDTO> Details { get; set; } = new List<TransferReceiveDetailDTO>();
    }

    public class TransferReceiveDetailDTO
    {
        public int TransferDetailId { get; set; }
        public decimal ReceivedQty { get; set; } // User inputs Box/Strip qty
    }
}
