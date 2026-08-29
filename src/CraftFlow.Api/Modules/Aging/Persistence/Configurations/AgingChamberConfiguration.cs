using CraftFlow.Api.Modules.Aging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Aging.Persistence.Configurations;

public sealed class AgingChamberConfiguration : IEntityTypeConfiguration<AgingChamber>
{
    public void Configure(EntityTypeBuilder<AgingChamber> builder)
    {
        builder.ToTable("AgingChambers", "aging");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.TargetTemperature).HasPrecision(5, 2);
        builder.Property(c => c.TargetHumidity).HasPrecision(5, 2);

        builder.HasQueryFilter(c => c.TenantId != Guid.Empty);
    }
}