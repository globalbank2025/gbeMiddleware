using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GBEMiddlewareApi.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class IdentityInitialAfterRoleAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the BranchId column to AspNetUsers.
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                schema: "middleware_schema",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Create the branch table if not already created.
            migrationBuilder.CreateTable(
                name: "branch",
                schema: "middleware_schema",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchCode = table.Column<string>(type: "text", nullable: false),
                    BranchName = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch", x => x.BranchId);
                });

            // Insert a default branch record (if not exists). 
            // Adjust the values as needed.
            migrationBuilder.Sql(@"
        INSERT INTO middleware_schema.branch (""BranchCode"", ""BranchName"", ""Location"")
        SELECT 'DEFAULT', 'Default Branch', 'Default Location'
        WHERE NOT EXISTS (
            SELECT 1 FROM middleware_schema.branch WHERE ""BranchCode"" = 'DEFAULT'
        );");

            // Update existing AspNetUsers rows (where BranchId = 0) to use the default branch.
            migrationBuilder.Sql(@"
        UPDATE middleware_schema.""AspNetUsers""
        SET ""BranchId"" = (
            SELECT ""BranchId"" FROM middleware_schema.branch WHERE ""BranchCode"" = 'DEFAULT'
        )
        WHERE ""BranchId"" = 0;
    ");

            // Create an index on BranchId in AspNetUsers.
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BranchId",
                schema: "middleware_schema",
                table: "AspNetUsers",
                column: "BranchId");

            // Finally, add the foreign key constraint.
            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_branch_BranchId",
                schema: "middleware_schema",
                table: "AspNetUsers",
                column: "BranchId",
                principalSchema: "middleware_schema",
                principalTable: "branch",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_branch_BranchId",
                schema: "middleware_schema",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "branch",
                schema: "middleware_schema");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BranchId",
                schema: "middleware_schema",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "middleware_schema",
                table: "AspNetUsers");
        }
    }
}
