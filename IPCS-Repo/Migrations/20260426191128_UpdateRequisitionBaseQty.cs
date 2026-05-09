using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPCS_Repo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequisitionBaseQty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConversionFactor",
                table: "TransferRequisitionDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsInBox",
                table: "TransferRequisitionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RequestQtyInPcs",
                table: "TransferRequisitionDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversionFactor",
                table: "TransferRequisitionDetails");

            migrationBuilder.DropColumn(
                name: "IsInBox",
                table: "TransferRequisitionDetails");

            migrationBuilder.DropColumn(
                name: "RequestQtyInPcs",
                table: "TransferRequisitionDetails");
        }
    }
}
