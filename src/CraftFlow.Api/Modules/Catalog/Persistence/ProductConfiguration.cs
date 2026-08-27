using CraftFlow.Api.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Catalog.Persistence
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");
            builder.HasKey(p => p.Id);

            builder.HasIndex(p => new { p.TenantId, p.Id }).IsUnique();

            builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
            builder.Property(p => p.TenantId).IsRequired();
        }
    }
}
