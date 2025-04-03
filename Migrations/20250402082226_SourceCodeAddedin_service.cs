using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBEMiddlewareApi.Migrations
{
    /// <inheritdoc />
    public partial class SourceCodeAddedin_service : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceCode",
                schema: "middleware_schema",
                table: "service",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceCode",
                schema: "middleware_schema",
                table: "service");
        }
    }
}
