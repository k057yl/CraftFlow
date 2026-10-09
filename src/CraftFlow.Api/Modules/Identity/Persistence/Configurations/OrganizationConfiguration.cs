using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Enums.Identity;
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

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(OrganizationStatus.Active);

        builder.Property(o => o.CreatedAtUtc)
            .IsRequired();

        builder.Property(o => o.DeactivatedAtUtc);

        builder.Property(o => o.LastRetentionNoticeSentAtUtc);

        builder.Navigation(o => o.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.Members)
            .WithOne(m => m.Organization)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.Status, o.DeactivatedAtUtc })
            .HasDatabaseName(DbIndexes.Identity.IX_ORGANIZATIONS_RETENTION_CHECK);
    }
}