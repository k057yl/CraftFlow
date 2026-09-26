using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Catalog.Persistence.Configurations;
public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable(DbTables.RECIPE_INGREDIENTS, DbSchemas.CATALOG);

        builder.HasKey(ri => ri.Id);
    }
}
