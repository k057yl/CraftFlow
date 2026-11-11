using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Persistence.Configurations;

public sealed class AgingLotItemConfiguration : IEntityTypeConfiguration<AgingLotItem>
{
    public void Configure(EntityTypeBuilder<AgingLotItem> builder)
    {
        builder.ToTable(DbTables.AGING_LOT_ITEMS, DbSchemas.CHEESE_MAKING);

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

        builder.HasIndex(i => i.AgingLotId).HasDatabaseName(DbIndexes.Aging.IX_AGING_LOT_ITEMS_LOT);
    }
}