using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPCS_Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalUnitConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "SalesReturnMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "PurchaseReturnMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlobalUnitConversion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUnitId = table.Column<int>(type: "int", nullable: false),
                    ToUnitId = table.Column<int>(type: "int", nullable: false),
                    ConversionFactor = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalUnitConversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalUnitConversion_UOM_FromUnitId",
                        column: x => x.FromUnitId,
                        principalTable: "UOM",
                        principalColumn: "UOMId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GlobalUnitConversion_UOM_ToUnitId",
                        column: x => x.ToUnitId,
                        principalTable: "UOM",
                        principalColumn: "UOMId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMaster_PaymentMethodId",
                table: "SalesReturnMaster",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnMaster_PaymentMethodId",
                table: "PurchaseReturnMaster",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalUnitConversion_FromUnitId",
                table: "GlobalUnitConversion",
                column: "FromUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalUnitConversion_ToUnitId",
                table: "GlobalUnitConversion",
                column: "ToUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturnMaster_PaymentMethod_PaymentMethodId",
                table: "PurchaseReturnMaster",
                column: "PaymentMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "MethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnMaster_PaymentMethod_PaymentMethodId",
                table: "SalesReturnMaster",
                column: "PaymentMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "MethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturnMaster_PaymentMethod_PaymentMethodId",
                table: "PurchaseReturnMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnMaster_PaymentMethod_PaymentMethodId",
                table: "SalesReturnMaster");

            migrationBuilder.DropTable(
                name: "GlobalUnitConversion");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturnMaster_PaymentMethodId",
                table: "SalesReturnMaster");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturnMaster_PaymentMethodId",
                table: "PurchaseReturnMaster");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "SalesReturnMaster");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "PurchaseReturnMaster");
        }
    }
}
