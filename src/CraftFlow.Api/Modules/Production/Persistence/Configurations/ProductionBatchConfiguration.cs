using CraftFlow.Api.Modules.Production.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Production.Persistence.Configurations
{
    public class ProductionBatchConfiguration : IEntityTypeConfiguration<ProductionBatch>
    {
        public void Configure(EntityTypeBuilder<ProductionBatch> builder)
        {
            builder.ToTable("production_batches");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PlannedOutputQuantity)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(p => p.ActualOutputQuantity)
                .HasPrecision(18, 4);

            builder.Property(p => p.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasQueryFilter(p => p.TenantId == EF.Property<Guid>(p, "TenantId"));
        }
    }
}
