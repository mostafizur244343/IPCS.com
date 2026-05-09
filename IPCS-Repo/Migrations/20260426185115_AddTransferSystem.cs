using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPCS_Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferRequisition",
                columns: table => new
                {
                    RequisitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequisitionCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FromBranchId = table.Column<int>(type: "int", nullable: false),
                    ToBranchId = table.Column<int>(type: "int", nullable: false),
                    RequisitionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferRequisition", x => x.RequisitionId);
                    table.ForeignKey(
                        name: "FK_TransferRequisition_Branch_FromBranchId",
                        column: x => x.FromBranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferRequisition_Branch_ToBranchId",
                        column: x => x.ToBranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferMaster",
                columns: table => new
                {
                    TransferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RequisitionId = table.Column<int>(type: "int", nullable: true),
                    FromBranchId = table.Column<int>(type: "int", nullable: false),
                    ToBranchId = table.Column<int>(type: "int", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferMaster", x => x.TransferId);
                    table.ForeignKey(
                        name: "FK_TransferMaster_Branch_FromBranchId",
                        column: x => x.FromBranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferMaster_Branch_ToBranchId",
                        column: x => x.ToBranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferMaster_TransferRequisition_RequisitionId",
                        column: x => x.RequisitionId,
                        principalTable: "TransferRequisition",
                        principalColumn: "RequisitionId");
                });

            migrationBuilder.CreateTable(
                name: "TransferRequisitionDetails",
                columns: table => new
                {
                    RequisitionDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequisitionId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RequestQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferRequisitionDetails", x => x.RequisitionDetailId);
                    table.ForeignKey(
                        name: "FK_TransferRequisitionDetails_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferRequisitionDetails_TransferRequisition_RequisitionId",
                        column: x => x.RequisitionId,
                        principalTable: "TransferRequisition",
                        principalColumn: "RequisitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferRequisitionDetails_UOM_UOMId",
                        column: x => x.UOMId,
                        principalTable: "UOM",
                        principalColumn: "UOMId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferDetails",
                columns: table => new
                {
                    TransferDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    TransferQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferDetails", x => x.TransferDetailId);
                    table.ForeignKey(
                        name: "FK_TransferDetails_LotInfo_LotId",
                        column: x => x.LotId,
                        principalTable: "LotInfo",
                        principalColumn: "LotId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferDetails_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferDetails_TransferMaster_TransferId",
                        column: x => x.TransferId,
                        principalTable: "TransferMaster",
                        principalColumn: "TransferId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_LotId",
                table: "TransferDetails",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_ProductId",
                table: "TransferDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_TransferId",
                table: "TransferDetails",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferMaster_FromBranchId",
                table: "TransferMaster",
                column: "FromBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferMaster_RequisitionId",
                table: "TransferMaster",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferMaster_ToBranchId",
                table: "TransferMaster",
                column: "ToBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferMaster_TransferCode",
                table: "TransferMaster",
                column: "TransferCode");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequisition_FromBranchId",
                table: "TransferRequisition",
                column: "FromBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequisition_RequisitionCode",
                table: "TransferRequisition",
                column: "RequisitionCode");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequisition_ToBranchId",
                table: "TransferRequisition",
                column: "ToBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequisitionDetails_ProductId",
                table: "TransferRequisitionDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequisitionDetails_RequisitionId",
                table: "TransferRequisitionDetails",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequisitionDetails_UOMId",
                table: "TransferRequisitionDetails",
                column: "UOMId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferDetails");

            migrationBuilder.DropTable(
                name: "TransferRequisitionDetails");

            migrationBuilder.DropTable(
                name: "TransferMaster");

            migrationBuilder.DropTable(
                name: "TransferRequisition");
        }
    }
}
