using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPCS_Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteMasterTablesAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UOM",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StoreLocation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductUnitConversion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PaymentMethod",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Manufacturer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InvoicePayment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "GenericInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Category",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Branch",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UOM");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StoreLocation");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductUnitConversion");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Manufacturer");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InvoicePayment");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "GenericInfo");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Branch");
        }
    }
}
