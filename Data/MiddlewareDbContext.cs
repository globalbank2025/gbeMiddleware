using Microsoft.EntityFrameworkCore;
using GBEMiddlewareApi.Models;

namespace GBEMiddlewareApi.Data
{
    public class MiddlewareDbContext : DbContext
    {
        public MiddlewareDbContext(DbContextOptions<MiddlewareDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for all your manually created tables.
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceEndpoint> ServiceEndpoints { get; set; }
        //public DbSet<ReferenceData> ReferenceDatas { get; set; }
        public DbSet<ApiCredentials> ApiCredentials { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<ResponseLog> ResponseLogs { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionDetail> TransactionDetails { get; set; }
        public DbSet<MappingRule> MappingRules { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ScheduledTask> ScheduledTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Use the manually created schema for these tables.
            builder.HasDefaultSchema("middleware_schema");

            // Map each entity to its existing table. For example:
            builder.Entity<Partner>(entity =>
            {
                entity.ToTable("partner");
                entity.HasKey(p => p.PartnerId);
            });
            builder.Entity<Service>(entity =>
            {
                entity.ToTable("service");
                entity.HasKey(s => s.ServiceId);
            });
            builder.Entity<ServiceEndpoint>(entity =>
            {
                entity.ToTable("service_endpoint");
                entity.HasKey(se => se.EndpointId);
                entity.Property(se => se.ServiceId).HasColumnName("service_id");
                entity.HasOne(se => se.Service)
                      .WithMany() // or configure navigation if needed
                      .HasForeignKey(se => se.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_service_endpoint_service");
            });
            // Continue mapping for each table...

            //builder.Entity<ReferenceData>(entity =>
            //{
            //    entity.ToTable("reference_data");
            //    entity.HasKey(r => r.RefId);
            //});

            builder.Entity<ApiCredentials>(entity =>
            {
                entity.ToTable("api_credentials");
                entity.HasKey(a => a.ApiCredId);
                entity.HasOne(a => a.Partner)
                      .WithMany()
                      .HasForeignKey(a => a.PartnerId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_api_credentials_partner");
                entity.HasOne(a => a.Service)
                      .WithMany()
                      .HasForeignKey(a => a.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_api_credentials_service");
            });

            builder.Entity<RequestLog>(entity =>
            {
                entity.ToTable("request_log");
                entity.HasKey(r => r.RequestId);
                entity.HasOne(r => r.Partner)
                      .WithMany()
                      .HasForeignKey(r => r.PartnerId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_request_log_partner");
                entity.HasOne(r => r.Service)
                      .WithMany()
                      .HasForeignKey(r => r.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_request_log_service");
            });

            builder.Entity<ResponseLog>(entity =>
            {
                entity.ToTable("response_log");
                entity.HasKey(r => r.ResponseId);
                entity.HasOne(r => r.Request)
                      .WithMany()
                      .HasForeignKey(r => r.RequestId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_response_log_request");
            });

            builder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transaction");
                entity.HasKey(t => t.TransactionId);
                entity.HasOne(t => t.Request)
                      .WithMany()
                      .HasForeignKey(t => t.RequestId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_transaction_request");
                entity.HasOne(t => t.Partner)
                      .WithMany()
                      .HasForeignKey(t => t.PartnerId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_transaction_partner");
                entity.HasOne(t => t.Service)
                      .WithMany()
                      .HasForeignKey(t => t.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_transaction_service");
            });

            builder.Entity<TransactionDetail>(entity =>
            {
                entity.ToTable("transaction_detail");
                entity.HasKey(td => td.TxDetailId);
                entity.HasOne(td => td.Transaction)
                      .WithMany()
                      .HasForeignKey(td => td.TransactionId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_tx_detail_transaction");
            });

            builder.Entity<MappingRule>(entity =>
            {
                entity.ToTable("mapping_rule");
                entity.HasKey(m => m.MappingId);
                entity.HasOne(m => m.Service)
                      .WithMany()
                      .HasForeignKey(m => m.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_mapping_rule_service");
            });

            builder.Entity<ErrorLog>(entity =>
            {
                entity.ToTable("error_log");
                entity.HasKey(e => e.ErrorId);
                entity.HasOne(e => e.Transaction)
                      .WithMany()
                      .HasForeignKey(e => e.TransactionId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .HasConstraintName("fk_error_log_tx");
                entity.HasOne(e => e.Request)
                      .WithMany()
                      .HasForeignKey(e => e.RequestId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .HasConstraintName("fk_error_log_req");
            });

            builder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_log");
                entity.HasKey(a => a.AuditId);
            });

            builder.Entity<ScheduledTask>(entity =>
            {
                entity.ToTable("scheduled_task");
                entity.HasKey(s => s.TaskId);
            });
        }
    }
}
