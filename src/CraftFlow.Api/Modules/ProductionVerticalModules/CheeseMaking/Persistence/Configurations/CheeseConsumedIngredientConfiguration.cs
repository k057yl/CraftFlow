using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Persistence.Configurations;

public sealed class CheeseConsumedIngredientConfiguration : IEntityTypeConfiguration<CheeseConsumedIngredient>
{
    public void Configure(EntityTypeBuilder<CheeseConsumedIngredient> builder)
    {
        builder.ToTable(DbTables.CHEESE_CONSUMED_INGREDIENTS, DbSchemas.CHEESE_MAKING);

        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.CheeseBatchId)
            .HasDatabaseName(DbIndexes.CheeseMaking.IX_CHEESE_CONSUMED_INGREDIENTS_BATCH);
    }
}