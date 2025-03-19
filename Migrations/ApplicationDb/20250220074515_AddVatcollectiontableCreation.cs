using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GBEMiddlewareApi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddVatcollectiontableCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VatCollectionTransactions",
                schema: "middleware_schema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    CustomerVatRegistrationNo = table.Column<string>(type: "text", nullable: false),
                    CustomerTinNo = table.Column<string>(type: "text", nullable: false),
                    CustomerTelephone = table.Column<string>(type: "text", nullable: false),
                    TransferAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ServiceIncomeGl = table.Column<string>(type: "text", nullable: false),
                    ServiceCharge = table.Column<decimal>(type: "numeric", nullable: false),
                    VatOnServiceCharge = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatCollectionTransactions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VatCollectionTransactions",
                schema: "middleware_schema");
        }
    }
}
