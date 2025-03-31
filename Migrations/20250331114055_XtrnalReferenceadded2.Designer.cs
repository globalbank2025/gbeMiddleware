﻿// <auto-generated />
using System;
using GBEMiddlewareApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GBEMiddlewareApi.Migrations
{
    [DbContext(typeof(MiddlewareDbContext))]
    [Migration("20250331114055_XtrnalReferenceadded2")]
    partial class XtrnalReferenceadded2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("middleware_schema")
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GBEMiddlewareApi.Models.ApiCredentials", b =>
                {
                    b.Property<long>("ApiCredId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("ApiCredId"));

                    b.Property<string>("AllowedIp")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ApiKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ApiSecret")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("PartnerId")
                        .HasColumnType("bigint");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("ServiceId")
                        .HasColumnType("bigint");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("TokenExpiry")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ApiCredId");

                    b.HasIndex("PartnerId");

                    b.HasIndex("ServiceId");

                    b.ToTable("api_credentials", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.AuditLog", b =>
                {
                    b.Property<long>("AuditId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("AuditId"));

                    b.Property<DateTimeOffset>("ChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ChangedBy")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("ExternalRequest")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ExternalResponse")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NewData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OldData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Operation")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<long>("RecordId")
                        .HasColumnType("bigint");

                    b.Property<string>("TableName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("AuditId");

                    b.ToTable("audit_log", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.ErrorLog", b =>
                {
                    b.Property<long>("ErrorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("ErrorId"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ErrorCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ErrorMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ErrorSeverity")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("ErrorTimestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("RequestId")
                        .HasColumnType("bigint");

                    b.Property<string>("StackTrace")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long?>("TransactionId")
                        .HasColumnType("bigint");

                    b.HasKey("ErrorId");

                    b.HasIndex("RequestId");

                    b.HasIndex("TransactionId");

                    b.ToTable("error_log", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.MappingRule", b =>
                {
                    b.Property<long>("MappingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("MappingId"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Direction")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MappingRules")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("ServiceId")
                        .HasColumnType("bigint");

                    b.Property<string>("SourceFormat")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TargetFormat")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("MappingId");

                    b.HasIndex("ServiceId");

                    b.ToTable("mapping_rule", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.Partner", b =>
                {
                    b.Property<long>("PartnerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("PartnerId"));

                    b.Property<string>("ContactEmail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ContactPerson")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ContactPhone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PartnerCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PartnerName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("PartnerId");

                    b.ToTable("partner", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.RequestLog", b =>
                {
                    b.Property<long>("RequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("RequestId"));

                    b.Property<string>("ClientIp")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("HttpMethod")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("PartnerId")
                        .HasColumnType("bigint");

                    b.Property<string>("RequestHeaders")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RequestPayload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("RequestTimestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("ServiceId")
                        .HasColumnType("bigint");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("RequestId");

                    b.HasIndex("PartnerId");

                    b.HasIndex("ServiceId");

                    b.ToTable("request_log", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.ResponseLog", b =>
                {
                    b.Property<long>("ResponseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("ResponseId"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("HttpStatusCode")
                        .HasColumnType("integer");

                    b.Property<int?>("ProcessingTimeMs")
                        .HasColumnType("integer");

                    b.Property<long>("RequestId")
                        .HasColumnType("bigint");

                    b.Property<string>("ResponseHeaders")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ResponsePayload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("ResponseTimestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ResponseId");

                    b.HasIndex("RequestId");

                    b.ToTable("response_log", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.ScheduledTask", b =>
                {
                    b.Property<long>("TaskId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("TaskId"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CronExpression")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("LastRunTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("NextRunTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TaskName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("TaskId");

                    b.ToTable("scheduled_task", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.Service", b =>
                {
                    b.Property<long>("ServiceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("ServiceId"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OffsetAccNo")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ProductCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ServiceCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ServiceName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ServiceType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("ServiceId");

                    b.ToTable("service", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.ServiceEndpoint", b =>
                {
                    b.Property<long>("EndpointId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("EndpointId"));

                    b.Property<int?>("ConnectionTimeout")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EndpointUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Environment")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("HttpMethod")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("ReadTimeout")
                        .HasColumnType("integer");

                    b.Property<long>("ServiceId")
                        .HasColumnType("bigint")
                        .HasColumnName("service_id");

                    b.Property<string>("SoapAction")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("EndpointId");

                    b.HasIndex("ServiceId");

                    b.ToTable("service_endpoint", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.Transaction", b =>
                {
                    b.Property<long>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("TransactionId"));

                    b.Property<decimal?>("Amount")
                        .HasColumnType("numeric");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("PartnerId")
                        .HasColumnType("bigint");

                    b.Property<long>("RequestId")
                        .HasColumnType("bigint");

                    b.Property<long>("ServiceId")
                        .HasColumnType("bigint");

                    b.Property<string>("TransactionRef")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TransactionStatus")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TransactionType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("TransactionId");

                    b.HasIndex("PartnerId");

                    b.HasIndex("RequestId");

                    b.HasIndex("ServiceId");

                    b.ToTable("transaction", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.TransactionDetail", b =>
                {
                    b.Property<long>("TxDetailId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("TxDetailId"));

                    b.Property<DateTimeOffset?>("EndedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ErrorMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RequestPayload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ResponsePayload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("StepName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("StepSequence")
                        .HasColumnType("integer");

                    b.Property<long>("TransactionId")
                        .HasColumnType("bigint");

                    b.HasKey("TxDetailId");

                    b.HasIndex("TransactionId");

                    b.ToTable("transaction_detail", "middleware_schema");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.ApiCredentials", b =>
                {
                    b.HasOne("GBEMiddlewareApi.Models.Partner", "Partner")
                        .WithMany()
                        .HasForeignKey("PartnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GBEMiddlewareApi.Models.Service", "Service")
                        .WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Partner");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.ErrorLog", b =>
                {
                    b.HasOne("GBEMiddlewareApi.Models.RequestLog", "Request")
                        .WithMany()
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_error_log_req");

                    b.HasOne("GBEMiddlewareApi.Models.Transaction", "Transaction")
                        .WithMany()
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_error_log_tx");

                    b.Navigation("Request");

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.MappingRule", b =>
                {
                    b.HasOne("GBEMiddlewareApi.Models.Service", "Service")
                        .WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_mapping_rule_service");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.RequestLog", b =>
                {
                    b.HasOne("GBEMiddlewareApi.Models.Partner", "Partner")
                        .WithMany()
                        .HasForeignKey("PartnerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_request_log_partner");

                    b.HasOne("GBEMiddlewareApi.Models.Service", "Service")
                        .WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_request_log_service");

                    b.Navigation("Partner");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.ResponseLog", b =>
                {
                    b.HasOne("GBEMiddlewareApi.Models.RequestLog", "Request")
                        .WithMany()
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_response_log_request");

                    b.Navigation("Request");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.ServiceEndpoint", b =>
                {
                    b.HasOne("GBEMiddlewareApi.Models.Service", "Service")
                        .WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_service_endpoint_service");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.Transaction", b =>
                {
                    b.HasOne("GBEMiddlewareApi.Models.Partner", "Partner")
                        .WithMany()
                        .HasForeignKey("PartnerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_transaction_partner");

                    b.HasOne("GBEMiddlewareApi.Models.RequestLog", "Request")
                        .WithMany()
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_transaction_request");

                    b.HasOne("GBEMiddlewareApi.Models.Service", "Service")
                        .WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_transaction_service");

                    b.Navigation("Partner");

                    b.Navigation("Request");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("GBEMiddlewareApi.Models.TransactionDetail", b =>
                {
                    b.HasOne("GBEMiddlewareApi.Models.Transaction", "Transaction")
                        .WithMany()
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_tx_detail_transaction");

                    b.Navigation("Transaction");
                });
#pragma warning restore 612, 618
        }
    }
}
