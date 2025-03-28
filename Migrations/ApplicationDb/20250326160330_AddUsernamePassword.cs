using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBEMiddlewareApi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddUsernamePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                schema: "middleware_schema",
                table: "ApiCredentials",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                schema: "middleware_schema",
                table: "ApiCredentials",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                schema: "middleware_schema",
                table: "ApiCredentials");

            migrationBuilder.DropColumn(
                name: "Username",
                schema: "middleware_schema",
                table: "ApiCredentials");
        }
    }
}
