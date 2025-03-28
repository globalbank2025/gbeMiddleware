using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBEMiddlewareApi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class OffsetAccNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OffsetAccNo",
                schema: "middleware_schema",
                table: "Service",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                schema: "middleware_schema",
                table: "Service",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffsetAccNo",
                schema: "middleware_schema",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                schema: "middleware_schema",
                table: "Service");
        }
    }
}
