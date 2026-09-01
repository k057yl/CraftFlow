using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Aging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Aging.Persistence.Configurations;

public sealed class AgingLotConfiguration : IEntityTypeConfiguration<AgingLot>
{
    public void Configure(EntityTypeBuilder<AgingLot> builder)
    {
        builder.ToTable(DbTables.AGING_LOTS, DbSchemas.AGING);

        builder.HasKey(l => l.Id);
        builder.Property(l => l.BatchNumber).HasMaxLength(100).IsRequired();
        builder.Property(l => l.InitialQuantity).HasPrecision(18, 4);
        builder.Property(l => l.CurrentQuantity).HasPrecision(18, 4);
        builder.Property(l => l.Status).HasConversion<int>();

        builder.HasQueryFilter(l => l.TenantId != Guid.Empty);
    }
}