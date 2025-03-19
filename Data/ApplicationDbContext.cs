using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GBEMiddlewareApi.Models;
using System;

namespace GBEMiddlewareApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Existing Entities
        public DbSet<Branch> Branches { get; set; }
        public DbSet<ServiceIncomeGl> ServiceIncomeGls { get; set; }
        public DbSet<VatCollectionTransaction> VatCollectionTransactions { get; set; }

        // NEW: DbSet for Logging
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // NEW: DbSet for storing 3rd-party API credentials
        public DbSet<ApiCredentials> ApiCredentials { get; set; }

        // NEW: DbSet for our TransactionLog
        public DbSet<TransactionLog> TransactionLogs { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Let Identity framework handle default mappings
            base.OnModelCreating(builder);

            // Optional: Add composite key for RolePermissions if you want uniqueness 
            builder.Entity<RolePermission>()
               .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // Use default schema for Identity + other tables
            builder.HasDefaultSchema("middleware_schema");

            // -------------------------
            // Identity table mappings
            // -------------------------
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(e => e.BranchId).IsRequired();
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.Users)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<IdentityRole>(entity => { entity.ToTable("AspNetRoles"); });
            builder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("AspNetUserRoles"); });
            builder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("AspNetUserClaims"); });
            builder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("AspNetUserLogins"); });
            builder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("AspNetRoleClaims"); });
            builder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("AspNetUserTokens"); });

            // -------------------------
            // Branch
            // -------------------------
            builder.Entity<Branch>(entity =>
            {
                entity.ToTable("branch");
                entity.HasKey(b => b.BranchId);
            });

            // -------------------------
            // LogEntry
            // -------------------------
            builder.Entity<LogEntry>(entity =>
            {
                entity.ToTable("LogEntry");
                entity.HasKey(e => e.LogEntryId);

                entity.Property(e => e.Endpoint)
                      .HasMaxLength(200);

                entity.Property(e => e.RequestXml)
                      .IsRequired();

                entity.Property(e => e.ResponseXml);

                entity.Property(e => e.Initiator)
                      .HasMaxLength(100);

                entity.Property(e => e.Timestamp)
                      .HasDefaultValueSql("now()");
            });

            // -------------------------
            // ApiCredentials
            // -------------------------
            builder.Entity<ApiCredentials>(entity =>
            {
                entity.ToTable("ApiCredentials");
                entity.HasKey(e => e.ApiCredId);

                entity.Property(e => e.ApiKey)
                      .HasMaxLength(200);

                entity.Property(e => e.ApiSecret)
                      .HasMaxLength(200);

                entity.Property(e => e.Status)
                      .HasMaxLength(50)
                      .HasDefaultValue("ACTIVE");
            });

            // -------------------------
            // TransactionLog (NEW)
            // -------------------------
            builder.Entity<TransactionLog>(entity =>
            {
                entity.ToTable("TransactionLogs"); // Table name
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CustomerAccount)
                      .HasMaxLength(50);

                entity.Property(e => e.CustomerName)
                      .HasMaxLength(200);

                entity.Property(e => e.RequestPayload)
                      .HasColumnType("text"); // or varchar(max) depending on DB
                entity.Property(e => e.ResponsePayload)
                      .HasColumnType("text");

                entity.Property(e => e.TransactionReference)
                      .HasMaxLength(50);

                entity.Property(e => e.ApprovedBy)
                      .HasMaxLength(100);

                // If you want a relationship to VatCollectionTransaction
                entity.HasOne<VatCollectionTransaction>()
                      .WithMany()
                      .HasForeignKey(e => e.VatCollectionTransactionId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Example to set default for CreatedAt:
                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("NOW()");
            });
        }
    }
}
