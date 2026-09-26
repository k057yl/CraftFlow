using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Procurement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Procurement.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable(DbTables.SUPPLIERS, DbSchemas.PROCUREMENT);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.Email).HasMaxLength(100);

        builder.HasIndex(s => new { s.TenantId, s.Name })
            .IsUnique()
            .HasDatabaseName(DbIndexes.Procurement.IX_SUPPLIERS_TENANT_NAME);
    }
}