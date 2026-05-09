namespace IPCS_Model.DTOs
{
    public class StoreLocationDTO
    {
        public string? ShelfName { get; set; }
        public string? RowNumber { get; set; }
        public string? ColumnNumber { get; set; }
        public string? FloorNumber { get; set; }
        public string? RoomNumber { get; set; }
        public int? Capacity { get; set; }
        public string? Notes { get; set; }
        public int? BranchId { get; set; } 
        public bool IsActive { get; set; } = true;
    }
}