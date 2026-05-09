using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPCS_Repo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDailySummaryWithMethodBuckets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalBkash",
                table: "DailyTransactionSummary",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCard",
                table: "DailyTransactionSummary",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCash",
                table: "DailyTransactionSummary",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNagad",
                table: "DailyTransactionSummary",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalBkash",
                table: "DailyTransactionSummary");

            migrationBuilder.DropColumn(
                name: "TotalCard",
                table: "DailyTransactionSummary");

            migrationBuilder.DropColumn(
                name: "TotalCash",
                table: "DailyTransactionSummary");

            migrationBuilder.DropColumn(
                name: "TotalNagad",
                table: "DailyTransactionSummary");
        }
    }
}
