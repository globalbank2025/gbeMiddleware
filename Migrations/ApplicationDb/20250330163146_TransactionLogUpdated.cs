using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GBEMiddlewareApi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class TransactionLogUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "ApiCredentials",
            //    schema: "middleware_schema");

            //migrationBuilder.DropTable(
            //    name: "Partner",
            //    schema: "middleware_schema");

            //migrationBuilder.DropTable(
            //    name: "Service",
            //    schema: "middleware_schema");

            migrationBuilder.AlterColumn<int>(
                name: "VatCollectionTransactionId",
                schema: "middleware_schema",
                table: "TransactionLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                schema: "middleware_schema",
                table: "TransactionLogs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VatCollectionTransactionId",
                schema: "middleware_schema",
                table: "TransactionLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                schema: "middleware_schema",
                table: "TransactionLogs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            
            //migrationBuilder.CreateTable(
            //    name: "Service",
            //    schema: "middleware_schema",
            //    columns: table => new
            //    {
            //        ServiceId = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            //        Description = table.Column<string>(type: "text", nullable: false),
            //        OffsetAccNo = table.Column<string>(type: "text", nullable: false),
            //        ServiceCode = table.Column<string>(type: "text", nullable: false),
            //        ServiceName = table.Column<string>(type: "text", nullable: false),
            //        ServiceType = table.Column<string>(type: "text", nullable: false),
            //        Status = table.Column<string>(type: "text", nullable: false),
            //        UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Service", x => x.ServiceId);
            //    });

           
            migrationBuilder.CreateIndex(
                name: "IX_ApiCredentials_PartnerId",
                schema: "middleware_schema",
                table: "ApiCredentials",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiCredentials_ServiceId",
                schema: "middleware_schema",
                table: "ApiCredentials",
                column: "ServiceId");
        }
    }
}
