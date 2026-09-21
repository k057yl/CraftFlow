using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Inventory.Persistence;

public class StockLotConfiguration : IEntityTypeConfiguration<StockLot>
{
    public void Configure(EntityTypeBuilder<StockLot> builder)
    {
        builder.ToTable(DbTables.STOCK_LOTS);

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Quantity).HasPrecision(18, 4);
        builder.Property(sl => sl.UnitPrice).HasPrecision(18, 4);

        builder.OwnsMany(sl => sl.StorageLocations, locBuilder =>
        {
            locBuilder.ToTable(DbTables.STOCK_LOT_STORAGE_LOCATIONS);
            locBuilder.WithOwner().HasForeignKey(x => x.StockLotId);
            locBuilder.HasKey(x => new { x.StockLotId, x.StorageLocationId });
            locBuilder.Property(x => x.AllocatedQuantity).HasPrecision(18, 4);
        });
    }
}