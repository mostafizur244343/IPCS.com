namespace IPCS_Model.DTOs
{
    public class ProductDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? Strength { get; set; }
        public decimal MRP { get; set; }
        public decimal SalesPrice { get; set; }


        //Unit Fields
        public int BaseUOMId { get; set; }
        public int SelectedPurchaseUnitId { get; set; } // Which unit user entry in stock?


        // For Opening Stock
        public decimal OpeningQuantity { get; set; }
        public decimal OpeningCostPrice { get; set; }

        public int ReorderLevel { get; set; }
        public int MinOrderQuantity { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UOMId { get; set; }
        public int GenericId { get; set; }
        public int? LocationId { get; set; }
        public bool IsService { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}