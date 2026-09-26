using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Aging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Aging.Persistence.Configurations;

public sealed class AgingChamberConfiguration : IEntityTypeConfiguration<AgingChamber>
{
    public void Configure(EntityTypeBuilder<AgingChamber> builder)
    {
        builder.ToTable(DbTables.AGING_CHAMBERS, DbSchemas.AGING);

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.TargetTemperature).HasPrecision(5, 2);
        builder.Property(c => c.TargetHumidity).HasPrecision(5, 2);

        builder.HasIndex(c => new { c.TenantId, c.Name })
            .IsUnique()
            .HasDatabaseName(DbIndexes.Aging.IX_AGING_CHAMBERS_TENANT_NAME);
    }
}