using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IPCS_Model.Entities
{
    // Unique Index on BranchId and TransactionDate
    [Index(nameof(BranchId), nameof(TransactionDate), IsUnique = true, Name = "IX_DailySummary_Branch_Date")]
    public class DailyTransactionSummary
    {
        [Key]
        public long SummaryId { get; set; }

        [Required]
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [Required]
        [Column(TypeName = "date")] // Save Only  Date Not Time
        public DateTime TransactionDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCashSales { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCash { get; set; } // Current Net Cash In Hand

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBkash { get; set; } // Current Net Bkash Balance

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalNagad { get; set; } // Current Net Nagad Balance

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCard { get; set; } // Current Net Card/Bank Balance

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDueSales { get; set; } // How Much Due from That day sale

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCollection { get; set; } // Collection From Before Due

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPurchase { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalExpense { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDiscount { get; set; }

        // Computed Column: ClosingCash
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ClosingCash { get; private set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetProfit { get; set; }

        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}