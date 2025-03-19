using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBEMiddlewareApi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class ServiceGLCodeUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GlCode",
                schema: "middleware_schema",
                table: "ServiceIncomeGls",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7);

            migrationBuilder.AddColumn<int>(
                name: "CalculationType",
                schema: "middleware_schema",
                table: "ServiceIncomeGls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "FlatPrice",
                schema: "middleware_schema",
                table: "ServiceIncomeGls",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                schema: "middleware_schema",
                table: "ServiceIncomeGls",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "middleware_schema",
                table: "ServiceIncomeGls",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalculationType",
                schema: "middleware_schema",
                table: "ServiceIncomeGls");

            migrationBuilder.DropColumn(
                name: "FlatPrice",
                schema: "middleware_schema",
                table: "ServiceIncomeGls");

            migrationBuilder.DropColumn(
                name: "Rate",
                schema: "middleware_schema",
                table: "ServiceIncomeGls");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "middleware_schema",
                table: "ServiceIncomeGls");

            migrationBuilder.AlterColumn<string>(
                name: "GlCode",
                schema: "middleware_schema",
                table: "ServiceIncomeGls",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
