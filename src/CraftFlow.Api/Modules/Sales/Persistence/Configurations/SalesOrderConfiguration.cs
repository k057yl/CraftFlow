using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Sales.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Sales.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable(DbTables.SALES_ORDERS, DbSchemas.SALES);

        builder.HasKey(o => o.Id);

        builder.Property(o => o.TotalAmount).HasPrecision(18, 4);
        builder.Property(o => o.Status).HasConversion<int>();

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.CustomerId).HasDatabaseName(DbIndexes.Sales.IX_SALES_ORDERS_CUSTOMER);
        builder.HasIndex(o => o.Status).HasDatabaseName(DbIndexes.Sales.IX_SALES_ORDERS_STATUS);
    }
}