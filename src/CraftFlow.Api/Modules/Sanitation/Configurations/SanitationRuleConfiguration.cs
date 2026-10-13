using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Sanitation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Sanitation.Configurations;

public class SanitationRuleConfiguration : IEntityTypeConfiguration<SanitationRule>
{
    public void Configure(EntityTypeBuilder<SanitationRule> builder)
    {
        builder.ToTable(DbTables.SANITATION_RULES, DbSchemas.SANITATION);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RequiredChemicals)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.TargetType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.TriggerType)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(r => new { r.TenantId, r.TargetType, r.TargetId })
            .HasDatabaseName(DbIndexes.Sanitation.IX_SANITATION_RULES_TENANT_TARGET);
    }
}