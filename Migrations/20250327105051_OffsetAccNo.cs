using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBEMiddlewareApi.Migrations
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
                table: "service",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                schema: "middleware_schema",
                table: "service",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "TableName",
                schema: "middleware_schema",
                table: "audit_log",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Operation",
                schema: "middleware_schema",
                table: "audit_log",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ChangedBy",
                schema: "middleware_schema",
                table: "audit_log",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ExternalRequest",
                schema: "middleware_schema",
                table: "audit_log",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalResponse",
                schema: "middleware_schema",
                table: "audit_log",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                schema: "middleware_schema",
                table: "api_credentials",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                schema: "middleware_schema",
                table: "api_credentials",
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
                table: "service");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                schema: "middleware_schema",
                table: "service");

            migrationBuilder.DropColumn(
                name: "ExternalRequest",
                schema: "middleware_schema",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "ExternalResponse",
                schema: "middleware_schema",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "Password",
                schema: "middleware_schema",
                table: "api_credentials");

            migrationBuilder.DropColumn(
                name: "Username",
                schema: "middleware_schema",
                table: "api_credentials");

            migrationBuilder.AlterColumn<string>(
                name: "TableName",
                schema: "middleware_schema",
                table: "audit_log",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Operation",
                schema: "middleware_schema",
                table: "audit_log",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ChangedBy",
                schema: "middleware_schema",
                table: "audit_log",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
