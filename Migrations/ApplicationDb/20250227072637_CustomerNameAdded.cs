using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBEMiddlewareApi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class CustomerNameAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                schema: "middleware_schema",
                table: "VatCollectionTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDateTime",
                schema: "middleware_schema",
                table: "VatCollectionTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                schema: "middleware_schema",
                table: "VatCollectionTransactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceChargeReference",
                schema: "middleware_schema",
                table: "VatCollectionTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatReference",
                schema: "middleware_schema",
                table: "VatCollectionTransactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "middleware_schema",
                table: "VatCollectionTransactions");

            migrationBuilder.DropColumn(
                name: "ApprovedDateTime",
                schema: "middleware_schema",
                table: "VatCollectionTransactions");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                schema: "middleware_schema",
                table: "VatCollectionTransactions");

            migrationBuilder.DropColumn(
                name: "ServiceChargeReference",
                schema: "middleware_schema",
                table: "VatCollectionTransactions");

            migrationBuilder.DropColumn(
                name: "VatReference",
                schema: "middleware_schema",
                table: "VatCollectionTransactions");
        }
    }
}
