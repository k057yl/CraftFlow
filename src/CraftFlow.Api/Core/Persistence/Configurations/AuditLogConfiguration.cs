using CraftFlow.Api.Core.Audit;
using CraftFlow.Api.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Core.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(DbTables.AUDIT_LOGS, DbSchemas.IDENTITY);
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
        builder.Property(a => a.UserEmail).HasMaxLength(256);
        builder.Property(a => a.Details).IsRequired();
        builder.Property(a => a.ChangesJson).HasColumnType("jsonb");

        builder.HasIndex(a => new { a.TenantId, a.CreatedAtUtc })
            .HasDatabaseName(DbIndexes.Identity.IX_AUDIT_LOGS_TENANT_CREATED_AT);
    }
}