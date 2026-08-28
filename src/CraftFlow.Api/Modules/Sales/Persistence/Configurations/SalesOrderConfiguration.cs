using CraftFlow.Api.Modules.Sales.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Sales.Persistence.Configurations
{
    public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
    {
        public void Configure(EntityTypeBuilder<SalesOrder> builder)
        {
            builder.ToTable("sales_orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
            builder.Property(o => o.Status).HasConversion<int>();
            builder.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
            builder.HasQueryFilter(o => o.TenantId == EF.Property<Guid>(o, "TenantId"));
        }
    }
}
