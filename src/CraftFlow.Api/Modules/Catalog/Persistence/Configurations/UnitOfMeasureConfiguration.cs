using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Catalog.Persistence.Configurations;
public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable(DbTables.UNITS_OF_MEASURE, DbSchemas.CATALOG);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.ConversionFactor)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasForeignKey(u => u.BaseUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
