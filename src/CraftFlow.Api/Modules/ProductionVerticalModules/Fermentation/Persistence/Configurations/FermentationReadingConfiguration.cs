using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Persistence.Configurations;

public sealed class FermentationReadingConfiguration : IEntityTypeConfiguration<FermentationReading>
{
    public void Configure(EntityTypeBuilder<FermentationReading> builder)
    {
        builder.ToTable(DbTables.FERMENTATION_READINGS, DbSchemas.FERMENTATION);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecordedAt).IsRequired();
        builder.Property(x => x.Ph).HasPrecision(4, 2);
        builder.Property(x => x.Temperature).HasPrecision(5, 2);
        builder.Property(x => x.DegassedOrBurped).IsRequired();
        builder.Property(x => x.BrineToppedUp).IsRequired();
        builder.Property(x => x.SensoryNotes).HasMaxLength(500);

        builder.HasIndex(x => new { x.FermentationBatchId, x.RecordedAt })
            .HasDatabaseName(DbIndexes.Fermentation.IX_FERMENTATION_READINGS_BATCH_RECORDED_AT);
    }
}