using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPCS_Repo.Migrations
{
    /// <inheritdoc />
    public partial class FixInvoicePaymentPurchaseLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserve the purchase link of payments saved through
            // PurchaseMaster.Payments: those rows carry the link only in the
            // shadow column PurchaseMasterPurchaseId while PurchaseId is NULL.
            migrationBuilder.Sql(
                "UPDATE [InvoicePayment] SET [PurchaseId] = [PurchaseMasterPurchaseId] " +
                "WHERE [PurchaseId] IS NULL AND [PurchaseMasterPurchaseId] IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayment_PurchaseMaster_PurchaseMasterPurchaseId",
                table: "InvoicePayment");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayment_PurchaseMasterPurchaseId",
                table: "InvoicePayment");

            migrationBuilder.DropColumn(
                name: "PurchaseMasterPurchaseId",
                table: "InvoicePayment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseMasterPurchaseId",
                table: "InvoicePayment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayment_PurchaseMasterPurchaseId",
                table: "InvoicePayment",
                column: "PurchaseMasterPurchaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayment_PurchaseMaster_PurchaseMasterPurchaseId",
                table: "InvoicePayment",
                column: "PurchaseMasterPurchaseId",
                principalTable: "PurchaseMaster",
                principalColumn: "PurchaseId");
        }
    }
}
