using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Sales.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Sales.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable(DbTables.CUSTOMERS, DbSchemas.SALES);

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(50);

        builder.HasIndex(c => new { c.TenantId, c.Name })
            .HasDatabaseName(DbIndexes.Sales.IX_CUSTOMERS_TENANT_NAME);
    }
}