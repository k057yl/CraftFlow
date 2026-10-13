using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Sanitation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Sanitation.Configurations;

public class SanitationLogConfiguration : IEntityTypeConfiguration<SanitationLog>
{
    public void Configure(EntityTypeBuilder<SanitationLog> builder)
    {
        builder.ToTable(DbTables.SANITATION_LOGS, DbSchemas.SANITATION);

        builder.HasKey(l => l.Id);

        builder.Property(l => l.TargetType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.ChemicalsUsed)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Notes)
            .HasMaxLength(1000);

        builder.Property(l => l.VerificationNotes)
            .HasMaxLength(1000);

        builder.HasIndex(l => new { l.TenantId, l.TaskId })
            .HasDatabaseName(DbIndexes.Sanitation.IX_SANITATION_LOGS_TENANT_TASK);

        builder.HasIndex(l => new { l.TenantId, l.TargetType, l.TargetId })
            .HasDatabaseName(DbIndexes.Sanitation.IX_SANITATION_LOGS_TENANT_TARGET);
    }
}