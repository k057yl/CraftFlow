using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Catalog.Persistence.Configurations;
public class RawMaterialConfiguration : IEntityTypeConfiguration<RawMaterial>
{
    public void Configure(EntityTypeBuilder<RawMaterial> builder)
    {
        builder.ToTable(DbTables.RAW_MATERIALS, DbSchemas.CATALOG);

        builder.HasKey(r => r.Id);
    }
}
