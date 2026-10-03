using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Identity.Domain;
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

        builder.Property(o => o.IsSelfDeactivated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(o => o.DeactivatedAtUtc);

        builder.Property(o => o.LastRetentionNoticeSentAtUtc);

        builder.Navigation(o => o.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.Members)
            .WithOne(m => m.Organization)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.IsActive, o.IsSelfDeactivated, o.DeactivatedAtUtc })
            .HasDatabaseName(DbIndexes.Identity.IX_ORGANIZATIONS_RETENTION_CHECK);
    }
}