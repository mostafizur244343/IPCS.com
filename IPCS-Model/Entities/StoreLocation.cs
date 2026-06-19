using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class StoreLocation
    {
        [Key]
        public int LocationId { get; set; }

        public string? ShelfName { get; set; }
        public string? RowNumber { get; set; }
        public string? ColumnNumber { get; set; }
        public string? FloorNumber { get; set; }
        public string? RoomNumber { get; set; }
        public int? Capacity { get; set; }
        public string? Notes { get; set; }

        // Realation with Branch (Nullable)
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; } 

        [NotMapped]
        public string? LocationName
        {
            get => ShelfName;
            set => ShelfName = value;
        }

        [NotMapped]
        public string? Description
        {
            get => Notes;
            set => Notes = value;
        }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}