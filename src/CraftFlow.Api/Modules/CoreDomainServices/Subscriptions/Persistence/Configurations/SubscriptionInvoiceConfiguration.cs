using CraftFlow.Api.Core.Constants;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Persistence.Configurations;

public class SubscriptionInvoiceConfiguration : IEntityTypeConfiguration<SubscriptionInvoice>
{
    public void Configure(EntityTypeBuilder<SubscriptionInvoice> builder)
    {
        builder.ToTable(DbTables.SUBSCRIPTION_INVOICES, DbSchemas.SAAS);

        builder.HasKey(i => i.Id);

        builder.Property(i => i.PlanCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Amount)
            .HasPrecision(18, 2);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(i => i.TenantId);
        builder.HasIndex(i => i.Status);
    }
}