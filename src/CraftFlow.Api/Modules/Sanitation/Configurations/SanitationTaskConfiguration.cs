using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Sanitation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Sanitation.Configurations;

public class SanitationTaskConfiguration : IEntityTypeConfiguration<SanitationTask>
{
    public void Configure(EntityTypeBuilder<SanitationTask> builder)
    {
        builder.ToTable(DbTables.SANITATION_TASKS, DbSchemas.SANITATION);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.TargetType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.ChemicalsUsed)
            .HasMaxLength(500);

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        builder.Property(t => t.VerificationNotes)
            .HasMaxLength(1000);

        builder.HasIndex(t => new { t.TenantId, t.Status })
            .HasDatabaseName(DbIndexes.Sanitation.IX_SANITATION_TASKS_TENANT_STATUS);

        builder.HasIndex(t => new { t.TenantId, t.TargetType, t.TargetId })
            .HasDatabaseName(DbIndexes.Sanitation.IX_SANITATION_TASKS_TENANT_TARGET);
    }
}