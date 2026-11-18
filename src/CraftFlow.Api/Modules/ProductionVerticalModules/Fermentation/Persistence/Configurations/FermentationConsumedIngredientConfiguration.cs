using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Persistence.Configurations;

public sealed class FermentationConsumedIngredientConfiguration : IEntityTypeConfiguration<FermentationConsumedIngredient>
{
    public void Configure(EntityTypeBuilder<FermentationConsumedIngredient> builder)
    {
        builder.ToTable(DbTables.FERMENTATION_CONSUMED_INGREDIENTS, DbSchemas.FERMENTATION);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.FermentationBatchId).IsRequired();
        builder.Property(x => x.StockLotId).IsRequired();
        builder.Property(x => x.RawMaterialId).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName(DbIndexes.Fermentation.IX_FERMENTATION_CONSUMED_INGREDIENTS_TENANT);

        builder.HasIndex(x => x.FermentationBatchId)
            .HasDatabaseName(DbIndexes.Fermentation.IX_FERMENTATION_CONSUMED_INGREDIENTS_BATCH);
    }
}