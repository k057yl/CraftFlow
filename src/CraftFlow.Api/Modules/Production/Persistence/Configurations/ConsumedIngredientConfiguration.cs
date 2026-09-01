using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Production.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CraftFlow.Api.Modules.Production.Persistence.Configurations;
public class ConsumedIngredientConfiguration : IEntityTypeConfiguration<ConsumedIngredient>
{
    public void Configure(EntityTypeBuilder<ConsumedIngredient> builder)
    {
        builder.ToTable(DbTables.CONSUMED_INGREDIENTS);
        builder.HasKey(c => c.Id);
    }
}
