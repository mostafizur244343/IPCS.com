using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPCS_Model.Entities
{
    public class GlobalUnitConversion
    {
        [Key]
        public int Id { get; set; }

        public int FromUnitId { get; set; } // Example: KG
        [ForeignKey("FromUnitId")]
        public virtual UOM? FromUnit { get; set; }

        public int ToUnitId { get; set; }   // Example: Gram
        [ForeignKey("ToUnitId")]
        public virtual UOM? ToUnit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ConversionFactor { get; set; } // Example: 1000

        public bool IsActive { get; set; } = true;
    }
}
