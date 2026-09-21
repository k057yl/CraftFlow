using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Inventory.Persistence.Configurations;

public class StorageLocationConfiguration : IEntityTypeConfiguration<StorageLocation>
{
    public void Configure(EntityTypeBuilder<StorageLocation> builder)
    {
        builder.ToTable(DbTables.STORAGE_LOCATIONS);

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sl => sl.LocationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sl => sl.Capacity)
            .HasPrecision(18, 4);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(sl => sl.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}