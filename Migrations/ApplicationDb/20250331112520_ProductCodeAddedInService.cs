using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBEMiddlewareApi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class ProductCodeAddedInService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalTransactionReference",
                schema: "middleware_schema",
                table: "TransactionLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalTransactionReference",
                schema: "middleware_schema",
                table: "TransactionLogs");
        }
    }
}
