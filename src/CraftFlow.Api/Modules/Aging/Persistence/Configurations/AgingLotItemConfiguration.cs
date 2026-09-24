using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Aging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Aging.Persistence.Configurations;

public sealed class AgingLotItemConfiguration : IEntityTypeConfiguration<AgingLotItem>
{
    public void Configure(EntityTypeBuilder<AgingLotItem> builder)
    {
        builder.ToTable(DbTables.AGING_LOT_ITEMS, DbSchemas.AGING);

        builder.HasKey(i => i.Id);

        builder.Property(i => i.AgingLotId)
            .IsRequired();

        builder.Property(i => i.ItemNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.InitialWeight)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(i => i.CurrentWeight)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(i => i.State)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.DiscardReason)
            .HasMaxLength(500)
            .IsRequired(false);
    }
}