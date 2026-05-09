using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPCS_Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferAuditAndUOM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TransferRequisition",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedBy",
                table: "TransferMaster",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedDate",
                table: "TransferMaster",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransferQtyInPcs",
                table: "TransferDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UOMId",
                table: "TransferDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_UOMId",
                table: "TransferDetails",
                column: "UOMId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferDetails_UOM_UOMId",
                table: "TransferDetails",
                column: "UOMId",
                principalTable: "UOM",
                principalColumn: "UOMId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransferDetails_UOM_UOMId",
                table: "TransferDetails");

            migrationBuilder.DropIndex(
                name: "IX_TransferDetails_UOMId",
                table: "TransferDetails");

            migrationBuilder.DropColumn(
                name: "ReceivedBy",
                table: "TransferMaster");

            migrationBuilder.DropColumn(
                name: "ReceivedDate",
                table: "TransferMaster");

            migrationBuilder.DropColumn(
                name: "TransferQtyInPcs",
                table: "TransferDetails");

            migrationBuilder.DropColumn(
                name: "UOMId",
                table: "TransferDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TransferRequisition",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
