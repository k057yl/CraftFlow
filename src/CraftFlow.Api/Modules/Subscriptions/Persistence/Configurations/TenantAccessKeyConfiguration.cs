using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Subscriptions.Persistence.Configurations;

public class TenantAccessKeyConfiguration : IEntityTypeConfiguration<TenantAccessKey>
{
    public void Configure(EntityTypeBuilder<TenantAccessKey> builder)
    {
        builder.ToTable(DbTables.TENANT_ACCESS_KEYS, DbSchemas.SAAS);

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(k => k.KeyPrefix)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(k => k.KeySuffix)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(k => k.KeyHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(k => k.KeyHash)
            .IsUnique()
            .HasDatabaseName(DbIndexes.Subscriptions.IX_ACCESS_KEYS_HASH);

        builder.Property(k => k.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(k => k.CreatedAtUtc).IsRequired();

        builder.HasOne(k => k.Subscription)
            .WithMany(s => s.Keys)
            .HasForeignKey(k => k.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(k => k.TenantId)
            .HasDatabaseName(DbIndexes.Subscriptions.IX_ACCESS_KEYS_TENANT);
    }
}