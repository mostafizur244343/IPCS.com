using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPCS_Repo.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequisitionConversionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInBox",
                table: "TransferRequisitionDetails");

            migrationBuilder.RenameColumn(
                name: "ConversionFactor",
                table: "TransferRequisitionDetails",
                newName: "ApprovedQtyInPcs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApprovedQtyInPcs",
                table: "TransferRequisitionDetails",
                newName: "ConversionFactor");

            migrationBuilder.AddColumn<bool>(
                name: "IsInBox",
                table: "TransferRequisitionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
