using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class ProductUnitConversion
    {
        [Key]
        public int ConversionId { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int FromUnitId { get; set; } // Example: Box
        [ForeignKey("FromUnitId")]
        public virtual UOM? FromUnit { get; set; }

        public int ToUnitId { get; set; }   //Example: Strip
        [ForeignKey("ToUnitId")]
        public virtual UOM? ToUnit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Factor { get; set; } // 1 FromUnit = How ToUnit?

        public int Level { get; set; } // 1: Container, 2: Box etc
    }
}