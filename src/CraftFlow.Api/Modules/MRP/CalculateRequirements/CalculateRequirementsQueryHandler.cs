using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.MRP.Contracts;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.MRP.CalculateRequirements;
public sealed class CalculateRequirementsQueryHandler : IRequestHandler<CalculateRequirementsQuery, Result<MrpReportDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CalculateRequirementsQueryHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<MrpReportDto>> Handle(CalculateRequirementsQuery request, CancellationToken cancellationToken)
    {
        var plannedBatches = await _dbContext.ProductionBatches
            .Where(b => b.State == BatchState.Draft)
            .ToListAsync(cancellationToken);

        if (plannedBatches.Count == 0)
        {
            return Result.Success(new MrpReportDto(DateTime.UtcNow, []));
        }

        var recipeIds = plannedBatches.Select(b => b.RecipeId).Distinct().ToList();

        var recipes = await _dbContext.Recipes
            .Include(r => r.Ingredients)
            .Where(r => recipeIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        var rawMaterialNeeds = new Dictionary<Guid, decimal>();

        foreach (var batch in plannedBatches)
        {
            if (!recipes.TryGetValue(batch.RecipeId, out var recipe))
                continue;

            var multiplier = recipe.TargetOutputQuantity > 0
                ? batch.PlannedOutputQuantity / recipe.TargetOutputQuantity
                : 1;

            foreach (var ingredient in recipe.Ingredients)
            {
                var requiredQty = ingredient.Quantity * multiplier;

                if (rawMaterialNeeds.TryGetValue(ingredient.RawMaterialId, out var currentNeed))
                {
                    rawMaterialNeeds[ingredient.RawMaterialId] = currentNeed + requiredQty;
                }
                else
                {
                    rawMaterialNeeds[ingredient.RawMaterialId] = requiredQty;
                }
            }
        }

        var rawMaterialIds = rawMaterialNeeds.Keys.ToList();

        var stockBalances = await _dbContext.StockLots
            .Where(sl => rawMaterialIds.Contains(sl.ItemId))
            .GroupBy(sl => sl.ItemId)
            .Select(g => new { RawMaterialId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.RawMaterialId, x => x.TotalQuantity, cancellationToken);

        var rawMaterials = await _dbContext.RawMaterials
            .Where(rm => rawMaterialIds.Contains(rm.Id))
            .ToDictionaryAsync(rm => rm.Id, rm => rm.Name, cancellationToken);

        var requirements = new List<MaterialRequirementDto>();

        foreach (var (rawMaterialId, totalRequired) in rawMaterialNeeds)
        {
            stockBalances.TryGetValue(rawMaterialId, out var currentStock);
            var shortage = Math.Max(0, totalRequired - currentStock);

            requirements.Add(new MaterialRequirementDto(
                rawMaterialId,
                rawMaterials.GetValueOrDefault(rawMaterialId, string.Empty),
                totalRequired,
                currentStock,
                shortage,
                SuggestedPurchaseQuantity: shortage
            ));
        }

        return Result.Success(new MrpReportDto(DateTime.UtcNow, requirements));
    }
}