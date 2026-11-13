using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Persistence.Configurations;

public sealed class FermentationBatchConfiguration : IEntityTypeConfiguration<FermentationBatch>
{
    public void Configure(EntityTypeBuilder<FermentationBatch> builder)
    {
        builder.ToTable(DbTables.FERMENTATION_BATCHES, DbSchemas.FERMENTATION);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BatchNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RecipeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SubstrateWeightKg).HasPrecision(18, 4);
        builder.Property(x => x.SaltPercentage).HasPrecision(6, 2);
        builder.Property(x => x.InitialPh).HasPrecision(4, 2);
        builder.Property(x => x.CurrentPh).HasPrecision(4, 2);
        builder.Property(x => x.TargetTemperature).HasPrecision(5, 2);
        builder.Property(x => x.VesselInfo).HasMaxLength(200);
        builder.Property(x => x.Stage).HasConversion<string>().HasMaxLength(40).IsRequired();

        builder.Ignore(x => x.CalculatedSaltGrams);

        builder.HasMany(x => x.Readings)
            .WithOne()
            .HasForeignKey(r => r.FermentationBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Readings).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new { x.TenantId, x.Stage })
            .HasDatabaseName(DbIndexes.Fermentation.IX_FERMENTATION_BATCHES_TENANT_STAGE);

        builder.HasIndex(x => x.ProductionBatchId)
            .HasDatabaseName(DbIndexes.Fermentation.IX_FERMENTATION_BATCHES_PRODUCTION_BATCH);
    }
}