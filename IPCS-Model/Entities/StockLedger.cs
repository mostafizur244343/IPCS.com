using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Model.Entities
{
    // Indexing for fast performence
    [Index(nameof(BranchId), nameof(ProductId), nameof(LotId), Name = "IX_Ledger_MainSearch")]
    public class StockLedger
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long LedgerId { get; set; } 

        [Required]
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        public int LotId { get; set; }
        [ForeignKey("LotId")]
        public virtual LotInfo? Lot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousBalance { get; set; } // Stock Before Transection

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityIn { get; set; } // How much coming(Purchase/Return)

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityOut { get; set; } // How much going (Sale/Damaged)

        // Computed Column: SQL Server Auto Calculated
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; private set; } // SQL Formula: (PreviousBalance + QuantityIn - QuantityOut)

        [StringLength(50)]
        public string? TransactionType { get; set; } // Purchase, Sale, Adjustment

        [StringLength(100)]
        public string? ReferenceNo { get; set; } // Invoice No 

        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string? Remarks { get; set; }
    }
}