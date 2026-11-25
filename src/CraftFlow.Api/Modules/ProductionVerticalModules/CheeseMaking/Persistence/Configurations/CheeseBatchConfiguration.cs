using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Persistence.Configurations;

public sealed class CheeseBatchConfiguration : IEntityTypeConfiguration<CheeseBatch>
{
    public void Configure(EntityTypeBuilder<CheeseBatch> builder)
    {
        builder.ToTable(DbTables.CHEESE_BATCHES, DbSchemas.CHEESE_MAKING);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.PlannedOutputQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(p => p.ActualOutputQuantity)
            .HasPrecision(18, 4);

        builder.Property(p => p.State)
            .HasConversion<int>()
            .IsRequired();

        builder.HasQueryFilter(p => p.TenantId == EF.Property<Guid>(p, "TenantId"));

        builder.HasIndex(b => new { b.TenantId, b.State, b.IsTelegramNotified, b.StartedAt })
            .HasDatabaseName(DbIndexes.CheeseMaking.IX_CHEESE_BATCHES_TIMER_MONITORING);
    }
}