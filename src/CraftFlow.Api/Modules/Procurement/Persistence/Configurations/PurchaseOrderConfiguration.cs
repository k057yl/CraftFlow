using CraftFlow.Api.Modules.Procurement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Procurement.Persistence.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders", "procurement");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(po => po.TotalAmount)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.HasMany(po => po.Items)
            .WithOne()
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(po => po.TenantId != Guid.Empty);
    }
}