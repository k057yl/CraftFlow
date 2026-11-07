using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.CoreDomainServices.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.Persistence.Configurations;

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable(DbTables.RECIPE_INGREDIENTS, DbSchemas.CATALOG);

        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.HasOne(ri => ri.Recipe)
            .WithMany(r => r.Ingredients)
            .HasForeignKey(ri => ri.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.RawMaterial)
            .WithMany()
            .HasForeignKey(ri => ri.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}