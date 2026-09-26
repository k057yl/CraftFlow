using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Sales.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Sales.Persistence.Configurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable(DbTables.SALES_ORDER_ITEMS, DbSchemas.SALES);
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.UnitPrice).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(s => s.SalesOrderId).HasDatabaseName(DbIndexes.Sales.IX_SALES_ORDER_ITEMS_ORDER);
    }
}