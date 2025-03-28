using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBEMiddlewareApi.Migrations
{
    /// <inheritdoc />
    public partial class ApiCredentialUpdatednew5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_api_credentials_partner",
                schema: "middleware_schema",
                table: "api_credentials");

            migrationBuilder.DropForeignKey(
                name: "fk_api_credentials_service",
                schema: "middleware_schema",
                table: "api_credentials");

            migrationBuilder.AddForeignKey(
                name: "FK_api_credentials_partner_PartnerId",
                schema: "middleware_schema",
                table: "api_credentials",
                column: "PartnerId",
                principalSchema: "middleware_schema",
                principalTable: "partner",
                principalColumn: "PartnerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_api_credentials_service_ServiceId",
                schema: "middleware_schema",
                table: "api_credentials",
                column: "ServiceId",
                principalSchema: "middleware_schema",
                principalTable: "service",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_api_credentials_partner_PartnerId",
                schema: "middleware_schema",
                table: "api_credentials");

            migrationBuilder.DropForeignKey(
                name: "FK_api_credentials_service_ServiceId",
                schema: "middleware_schema",
                table: "api_credentials");

            migrationBuilder.AddForeignKey(
                name: "fk_api_credentials_partner",
                schema: "middleware_schema",
                table: "api_credentials",
                column: "PartnerId",
                principalSchema: "middleware_schema",
                principalTable: "partner",
                principalColumn: "PartnerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_api_credentials_service",
                schema: "middleware_schema",
                table: "api_credentials",
                column: "ServiceId",
                principalSchema: "middleware_schema",
                principalTable: "service",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
