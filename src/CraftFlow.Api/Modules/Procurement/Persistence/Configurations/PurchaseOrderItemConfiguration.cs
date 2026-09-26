using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Procurement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Procurement.Persistence.Configurations;

public sealed class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable(DbTables.PURCHASE_ORDER_ITEMS, DbSchemas.PROCUREMENT);

        builder.HasKey(poi => poi.Id);

        builder.Property(poi => poi.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(poi => poi.UnitPrice).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(poi => poi.PurchaseOrderId).HasDatabaseName(DbIndexes.Procurement.IX_PURCHASE_ORDER_ITEMS_ORDER);
    }
}