using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.CoreDomainServices.Catalog.Domain;
using CraftFlow.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable(DbTables.RECIPES, DbSchemas.CATALOG);

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.TargetOutputQuantity).HasPrecision(18, 4);
        builder.Property(r => r.TenantId).IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(r => new { r.TenantId, r.ProductId })
            .HasPrincipalKey(p => new { p.TenantId, p.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.TargetModule)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(AppModule.CHEESE_MAKING)
            .IsRequired();

        builder.Navigation(r => r.Ingredients).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}