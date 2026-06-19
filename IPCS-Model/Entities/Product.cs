using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Enter Product Name")]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;
        public string? ProductCode { get; set; } // Auto Generated
        public string? SKU { get; set; }//Stock Keeping Unit
        public string? Strength { get; set; } // Example: 500mg
        public string? PicturePath { get; set; } // Product image

        [Column(TypeName = "decimal(18,2)")]
        public decimal MRP { get; set; } // Maximum Retail Price
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalesPrice { get; set; } // Current Sales Price


        // Multi-Unit Conversion Fields
        // (Base Unit)
        public int BaseUOMId { get; set; }
        [ForeignKey("BaseUOMId")]
        public virtual UOM? BaseUOM { get; set; }
        
        // Base to base system
        public virtual ICollection<ProductUnitConversion> UnitConversions { get; set; } = new List<ProductUnitConversion>();

        // Future Plan For Moving Average Logic
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; } // Moving Average Cost

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentStock { get; set; } // Global total stock across all branches/lots

        // Alert Field
        public int ReorderLevel { get; set; }
        public int MinOrderQuantity { get; set; }

        // Foreign Keys
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")] public virtual Category? Category { get; set; }

        public int BrandId { get; set; }
        [ForeignKey("BrandId")] public virtual Manufacturer? Brand { get; set; }

        public int UOMId { get; set; }
        [ForeignKey("UOMId")] public virtual UOM? UOM { get; set; }

        public int GenericId { get; set; }
        [ForeignKey("GenericId")] public virtual GenericInfo? Generic { get; set; }

        public int? LocationId { get; set; }
        [ForeignKey("LocationId")] public virtual StoreLocation? Location { get; set; }

        public bool IsService { get; set; } = false; // If true, stock won't be deducted
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false; // As Customer and Supplier Soft Delete System
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}