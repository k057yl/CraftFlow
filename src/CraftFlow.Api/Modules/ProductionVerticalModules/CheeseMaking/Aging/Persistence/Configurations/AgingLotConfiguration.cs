using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Persistence.Configurations;

public sealed class AgingLotConfiguration : IEntityTypeConfiguration<AgingLot>
{
    public void Configure(EntityTypeBuilder<AgingLot> builder)
    {
        builder.ToTable(DbTables.AGING_LOTS, DbSchemas.CHEESE_MAKING);

        builder.HasKey(l => l.Id);

        builder.Property(l => l.BatchNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.InitialQuantity)
            .HasPrecision(18, 4);

        builder.Property(l => l.State)
            .HasConversion<int>();

        builder.Ignore(l => l.CurrentQuantity);
        builder.Ignore(l => l.UnitsCount);

        builder.HasMany(l => l.Items)
            .WithOne()
            .HasForeignKey(i => i.AgingLotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(l => l.AgingChamberId)
            .HasDatabaseName(DbIndexes.Aging.IX_AGING_LOTS_CHAMBER);

        builder.HasIndex(l => l.ProductionBatchId)
            .HasDatabaseName(DbIndexes.Aging.IX_AGING_LOTS_BATCH);

        builder.HasIndex(l => new { l.TenantId, l.State })
            .HasDatabaseName(DbIndexes.Aging.IX_AGING_LOTS_TENANT_STATE);
    }
}