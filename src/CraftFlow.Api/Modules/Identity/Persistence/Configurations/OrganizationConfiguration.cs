using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Identity.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable(DbTables.ORGANIZATIONS, DbSchemas.IDENTITY);

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(o => o.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(o => new { o.IsActive, o.IsSelfDeactivated, o.DeactivatedAtUtc })
            .HasDatabaseName(DbIndexes.Identity.IX_ORGANIZATIONS_RETENTION_CHECK);
    }
}