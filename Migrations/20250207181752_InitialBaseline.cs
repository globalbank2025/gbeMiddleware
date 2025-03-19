using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GBEMiddlewareApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "middleware_schema");

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "middleware_schema",
                columns: table => new
                {
                    AuditId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    RecordId = table.Column<long>(type: "bigint", nullable: false),
                    Operation = table.Column<string>(type: "text", nullable: false),
                    OldData = table.Column<string>(type: "text", nullable: false),
                    NewData = table.Column<string>(type: "text", nullable: false),
                    ChangedBy = table.Column<string>(type: "text", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "partner",
                schema: "middleware_schema",
                columns: table => new
                {
                    PartnerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartnerCode = table.Column<string>(type: "text", nullable: false),
                    PartnerName = table.Column<string>(type: "text", nullable: false),
                    ContactPerson = table.Column<string>(type: "text", nullable: false),
                    ContactEmail = table.Column<string>(type: "text", nullable: false),
                    ContactPhone = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner", x => x.PartnerId);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_task",
                schema: "middleware_schema",
                columns: table => new
                {
                    TaskId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskName = table.Column<string>(type: "text", nullable: false),
                    CronExpression = table.Column<string>(type: "text", nullable: false),
                    LastRunTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextRunTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_task", x => x.TaskId);
                });

            migrationBuilder.CreateTable(
                name: "service",
                schema: "middleware_schema",
                columns: table => new
                {
                    ServiceId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceCode = table.Column<string>(type: "text", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "api_credentials",
                schema: "middleware_schema",
                columns: table => new
                {
                    ApiCredId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartnerId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    ApiSecret = table.Column<string>(type: "text", nullable: false),
                    TokenExpiry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AllowedIp = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_credentials", x => x.ApiCredId);
                    table.ForeignKey(
                        name: "fk_api_credentials_partner",
                        column: x => x.PartnerId,
                        principalSchema: "middleware_schema",
                        principalTable: "partner",
                        principalColumn: "PartnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_api_credentials_service",
                        column: x => x.ServiceId,
                        principalSchema: "middleware_schema",
                        principalTable: "service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mapping_rule",
                schema: "middleware_schema",
                columns: table => new
                {
                    MappingId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    SourceFormat = table.Column<string>(type: "text", nullable: false),
                    TargetFormat = table.Column<string>(type: "text", nullable: false),
                    MappingRules = table.Column<string>(type: "text", nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapping_rule", x => x.MappingId);
                    table.ForeignKey(
                        name: "fk_mapping_rule_service",
                        column: x => x.ServiceId,
                        principalSchema: "middleware_schema",
                        principalTable: "service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "request_log",
                schema: "middleware_schema",
                columns: table => new
                {
                    RequestId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartnerId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    RequestTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HttpMethod = table.Column<string>(type: "text", nullable: false),
                    RequestHeaders = table.Column<string>(type: "text", nullable: false),
                    RequestPayload = table.Column<string>(type: "text", nullable: false),
                    ClientIp = table.Column<string>(type: "text", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_request_log", x => x.RequestId);
                    table.ForeignKey(
                        name: "fk_request_log_partner",
                        column: x => x.PartnerId,
                        principalSchema: "middleware_schema",
                        principalTable: "partner",
                        principalColumn: "PartnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_request_log_service",
                        column: x => x.ServiceId,
                        principalSchema: "middleware_schema",
                        principalTable: "service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "service_endpoint",
                schema: "middleware_schema",
                columns: table => new
                {
                    EndpointId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    Environment = table.Column<string>(type: "text", nullable: false),
                    EndpointUrl = table.Column<string>(type: "text", nullable: false),
                    HttpMethod = table.Column<string>(type: "text", nullable: false),
                    SoapAction = table.Column<string>(type: "text", nullable: false),
                    ConnectionTimeout = table.Column<int>(type: "integer", nullable: true),
                    ReadTimeout = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_endpoint", x => x.EndpointId);
                    table.ForeignKey(
                        name: "fk_service_endpoint_service",
                        column: x => x.service_id,
                        principalSchema: "middleware_schema",
                        principalTable: "service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "response_log",
                schema: "middleware_schema",
                columns: table => new
                {
                    ResponseId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    ResponseTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: false),
                    ResponseHeaders = table.Column<string>(type: "text", nullable: false),
                    ResponsePayload = table.Column<string>(type: "text", nullable: false),
                    ProcessingTimeMs = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_response_log", x => x.ResponseId);
                    table.ForeignKey(
                        name: "fk_response_log_request",
                        column: x => x.RequestId,
                        principalSchema: "middleware_schema",
                        principalTable: "request_log",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                schema: "middleware_schema",
                columns: table => new
                {
                    TransactionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    PartnerId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionRef = table.Column<string>(type: "text", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    TransactionStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction", x => x.TransactionId);
                    table.ForeignKey(
                        name: "fk_transaction_partner",
                        column: x => x.PartnerId,
                        principalSchema: "middleware_schema",
                        principalTable: "partner",
                        principalColumn: "PartnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transaction_request",
                        column: x => x.RequestId,
                        principalSchema: "middleware_schema",
                        principalTable: "request_log",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transaction_service",
                        column: x => x.ServiceId,
                        principalSchema: "middleware_schema",
                        principalTable: "service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "error_log",
                schema: "middleware_schema",
                columns: table => new
                {
                    ErrorId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionId = table.Column<long>(type: "bigint", nullable: true),
                    RequestId = table.Column<long>(type: "bigint", nullable: true),
                    ErrorTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ErrorSeverity = table.Column<string>(type: "text", nullable: false),
                    ErrorCode = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error_log", x => x.ErrorId);
                    table.ForeignKey(
                        name: "fk_error_log_req",
                        column: x => x.RequestId,
                        principalSchema: "middleware_schema",
                        principalTable: "request_log",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_error_log_tx",
                        column: x => x.TransactionId,
                        principalSchema: "middleware_schema",
                        principalTable: "transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "transaction_detail",
                schema: "middleware_schema",
                columns: table => new
                {
                    TxDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionId = table.Column<long>(type: "bigint", nullable: false),
                    StepSequence = table.Column<int>(type: "integer", nullable: false),
                    StepName = table.Column<string>(type: "text", nullable: false),
                    RequestPayload = table.Column<string>(type: "text", nullable: false),
                    ResponsePayload = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_detail", x => x.TxDetailId);
                    table.ForeignKey(
                        name: "fk_tx_detail_transaction",
                        column: x => x.TransactionId,
                        principalSchema: "middleware_schema",
                        principalTable: "transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_credentials_PartnerId",
                schema: "middleware_schema",
                table: "api_credentials",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_api_credentials_ServiceId",
                schema: "middleware_schema",
                table: "api_credentials",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_error_log_RequestId",
                schema: "middleware_schema",
                table: "error_log",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_error_log_TransactionId",
                schema: "middleware_schema",
                table: "error_log",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_mapping_rule_ServiceId",
                schema: "middleware_schema",
                table: "mapping_rule",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_request_log_PartnerId",
                schema: "middleware_schema",
                table: "request_log",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_request_log_ServiceId",
                schema: "middleware_schema",
                table: "request_log",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_response_log_RequestId",
                schema: "middleware_schema",
                table: "response_log",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_service_endpoint_service_id",
                schema: "middleware_schema",
                table: "service_endpoint",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_PartnerId",
                schema: "middleware_schema",
                table: "transaction",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_RequestId",
                schema: "middleware_schema",
                table: "transaction",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_ServiceId",
                schema: "middleware_schema",
                table: "transaction",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_detail_TransactionId",
                schema: "middleware_schema",
                table: "transaction_detail",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_credentials",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "error_log",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "mapping_rule",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "response_log",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "scheduled_task",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "service_endpoint",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "transaction_detail",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "transaction",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "request_log",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "partner",
                schema: "middleware_schema");

            migrationBuilder.DropTable(
                name: "service",
                schema: "middleware_schema");
        }
    }
}
