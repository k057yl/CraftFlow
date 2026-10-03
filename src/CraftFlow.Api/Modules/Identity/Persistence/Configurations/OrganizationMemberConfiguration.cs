using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Identity.Persistence.Configurations;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable(DbTables.TABLE_ORGANIZATION_MEMBERS, DbSchemas.IDENTITY);

        builder.HasKey(m => m.Id);

        builder.Property(m => m.TenantId)
            .IsRequired();

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.Property(m => m.Role)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(TenantRole.Technologist);

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.JoinedAtUtc)
            .IsRequired();

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.TenantId, m.UserId })
            .IsUnique()
            .HasDatabaseName(DbIndexes.Identity.IX_MEMBERS_TENANT_USER);

        builder.HasIndex(m => new { m.TenantId, m.Role, m.IsActive })
            .HasDatabaseName(DbIndexes.Identity.IX_MEMBERS_TENANT_ROLE_ACTIVE);
    }
}