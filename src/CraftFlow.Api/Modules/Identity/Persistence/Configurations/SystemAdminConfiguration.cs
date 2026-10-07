using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Identity.Persistence.Configurations;

public class SystemAdminConfiguration : IEntityTypeConfiguration<SystemAdmin>
{
    public void Configure(EntityTypeBuilder<SystemAdmin> builder)
    {
        builder.ToTable(DbTables.SYSTEM_ADMINS, DbSchemas.IDENTITY);

        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.UserId)
            .IsRequired();

        builder.Property(sa => sa.GrantedAtUtc)
            .IsRequired();

        builder.HasIndex(sa => sa.UserId)
            .IsUnique()
            .HasDatabaseName(DbIndexes.Identity.IX_SYSTEM_ADMINS_USER_ID);
    }
}