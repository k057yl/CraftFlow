using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Inventory.Persistence.Configurations;

public class StorageLocationConfiguration : IEntityTypeConfiguration<StorageLocation>
{
    public void Configure(EntityTypeBuilder<StorageLocation> builder)
    {
        builder.ToTable(DbTables.STORAGE_LOCATIONS, DbSchemas.INVENTORY);

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sl => sl.LocationType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sl => sl.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sl => sl.UnitOfMeasure)
            .HasMaxLength(10);

        builder.Property(sl => sl.Capacity)
            .HasPrecision(18, 4);

        builder.Property(sl => sl.CurrentVolume)
            .HasPrecision(18, 4);

        builder.Property(sl => sl.CurrentProductionBatchId)
            .IsRequired(false);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(sl => sl.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(sl => new { sl.TenantId, sl.WarehouseId })
            .HasDatabaseName(DbIndexes.Inventory.IX_STORAGE_LOCATIONS_TENANT_WAREHOUSE);
    }
}