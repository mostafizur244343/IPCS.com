using System;

namespace IPCS_Model.DTOs
{
    /// <summary>
    /// Named version of the anonymous shape BranchLotStockService.GetAllAsync
    /// returns, so the paged endpoint can carry it inside PagedResult&lt;T&gt;.
    /// Field-for-field identical to the unpaged shape.
    /// </summary>
    public class BranchLotStockListItem
    {
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? SKU { get; set; }
        public int UomId { get; set; }
        public string? UomName { get; set; }
        public int LotId { get; set; }
        public string? LotNumber { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal CurrentStockInPcs { get; set; }
        public decimal DamagedStock { get; set; }
        public string? PicturePath { get; set; }
        public DateTime? LastUpdated { get; set; }
        public decimal PurchasePrice { get; set; }
    }
}
