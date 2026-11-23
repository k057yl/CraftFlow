using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.Persistence.Configurations;

public sealed class StockLotItemConfiguration : IEntityTypeConfiguration<StockLotItem>
{
    public void Configure(EntityTypeBuilder<StockLotItem> builder)
    {
        builder.ToTable(DbTables.STOCK_LOT_ITEMS, DbSchemas.INVENTORY);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StockLotId)
            .IsRequired();

        builder.Property(x => x.ItemNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CurrentWeight)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.IsSold)
            .IsRequired();

        builder.HasIndex(x => x.StockLotId);
    }
}