using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Subscriptions.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable(DbTables.SUBSCRIPTION_PLANS);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.MaxMonthlyBatches).IsRequired();
        builder.Property(p => p.MaxWarehouses).IsRequired();
        builder.Property(p => p.MaxChambers).IsRequired();
        builder.Property(p => p.MaxUsers).IsRequired();
    }
}