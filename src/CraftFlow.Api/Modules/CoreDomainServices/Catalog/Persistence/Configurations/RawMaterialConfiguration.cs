using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.CoreDomainServices.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.Persistence.Configurations;
public class RawMaterialConfiguration : IEntityTypeConfiguration<RawMaterial>
{
    public void Configure(EntityTypeBuilder<RawMaterial> builder)
    {
        builder.ToTable(DbTables.RAW_MATERIALS, DbSchemas.CATALOG);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(r => r.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(r => r.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
