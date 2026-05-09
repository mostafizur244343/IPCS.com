using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPCS_Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesReturnModuleWithFinancialCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesReturnMaster",
                columns: table => new
                {
                    ReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefundType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnMaster", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_SalesReturnMaster_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturnMaster_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturnMaster_SalesMaster_SalesId",
                        column: x => x.SalesId,
                        principalTable: "SalesMaster",
                        principalColumn: "SalesId");
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetails",
                columns: table => new
                {
                    ReturnDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnDetails", x => x.ReturnDetailId);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_LotInfo_LotId",
                        column: x => x.LotId,
                        principalTable: "LotInfo",
                        principalColumn: "LotId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_SalesReturnMaster_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "SalesReturnMaster",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_UOM_UOMId",
                        column: x => x.UOMId,
                        principalTable: "UOM",
                        principalColumn: "UOMId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_LotId",
                table: "SalesReturnDetails",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_ProductId",
                table: "SalesReturnDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_ReturnId",
                table: "SalesReturnDetails",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_UOMId",
                table: "SalesReturnDetails",
                column: "UOMId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMaster_BranchId",
                table: "SalesReturnMaster",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMaster_CustomerId",
                table: "SalesReturnMaster",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMaster_ReturnNo",
                table: "SalesReturnMaster",
                column: "ReturnNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMaster_SalesId",
                table: "SalesReturnMaster",
                column: "SalesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesReturnDetails");

            migrationBuilder.DropTable(
                name: "SalesReturnMaster");
        }
    }
}
