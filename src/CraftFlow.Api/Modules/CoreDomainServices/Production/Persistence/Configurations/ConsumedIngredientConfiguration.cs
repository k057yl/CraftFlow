using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.CoreDomainServices.Production.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.CoreDomainServices.Production.Persistence.Configurations;

public class ConsumedIngredientConfiguration : IEntityTypeConfiguration<ConsumedIngredient>
{
    public void Configure(EntityTypeBuilder<ConsumedIngredient> builder)
    {
        builder.ToTable(DbTables.CONSUMED_INGREDIENTS, DbSchemas.PRODUCTION);

        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.ProductionBatchId)
            .HasDatabaseName(DbIndexes.Production.IX_CONSUMED_INGREDIENTS_BATCH);
    }
}