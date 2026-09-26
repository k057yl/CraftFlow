using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Common.Persistence.Configurations;

public class TenantAccessKeyConfiguration : IEntityTypeConfiguration<TenantAccessKey>
{
    public void Configure(EntityTypeBuilder<TenantAccessKey> builder)
    {
        builder.ToTable(DbTables.TENANT_ACCESS_KEYS, DbSchemas.SAAS);
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.KeyHash).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.KeyPrefix).HasMaxLength(16).IsRequired();
        builder.Property(x => x.KeySuffix).HasMaxLength(8).IsRequired();
        builder.Property(x => x.KeyHash).HasMaxLength(64).IsRequired();

        builder.HasOne(x => x.Subscription)
            .WithMany(s => s.Keys)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}