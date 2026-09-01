using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Inventory.Persistence.Configurations;

public class StockLotConfiguration : IEntityTypeConfiguration<StockLot>
{
    public void Configure(EntityTypeBuilder<StockLot> builder)
    {
        builder.ToTable(DbTables.STOCK_LOTS);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(s => s.BatchNumber)
            .HasMaxLength(100);

        builder.HasQueryFilter(s => s.TenantId == EF.Property<Guid>(s, "TenantId"));
    }
}