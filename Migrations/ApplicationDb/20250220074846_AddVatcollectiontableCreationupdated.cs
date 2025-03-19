using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBEMiddlewareApi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddVatcollectiontableCreationupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransferAmount",
                schema: "middleware_schema",
                table: "VatCollectionTransactions",
                newName: "PrincipalAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrincipalAmount",
                schema: "middleware_schema",
                table: "VatCollectionTransactions",
                newName: "TransferAmount");
        }
    }
}
