using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.CalculateRequirements;

public class CalculateRequirementsHandler : IRequestHandler<CalculateBatchRequirementsQuery, Result<List<RequirementItemDto>>>
{
    private readonly AppDbContext _dbContext;

    public CalculateRequirementsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<RequirementItemDto>>> Handle(CalculateBatchRequirementsQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);

        if (recipe == null || recipe.TargetOutputQuantity <= 0 || recipe.Ingredients == null || !recipe.Ingredients.Any())
        {
            return Result.Success(new List<RequirementItemDto>());
        }

        var rawMaterialIds = recipe.Ingredients.Select(i => i.RawMaterialId).ToList();
        var rawMaterials = await _dbContext.RawMaterials
            .AsNoTracking()
            .Include(rm => rm.UnitOfMeasure)
            .Where(rm => rawMaterialIds.Contains(rm.Id))
            .ToDictionaryAsync(rm => rm.Id, cancellationToken);

        var stockLots = await _dbContext.StockLots
            .AsNoTracking()
            .Include(s => s.UnitOfMeasure)
            .Where(s => s.WarehouseId == request.WarehouseId && rawMaterialIds.Contains(s.ItemId) && s.Quantity > 0)
            .ToListAsync(cancellationToken);

        var multiplier = request.PlannedQty / recipe.TargetOutputQuantity;
        var items = new List<RequirementItemDto>();

        foreach (var ingredient in recipe.Ingredients)
        {
            var rawMaterial = rawMaterials.GetValueOrDefault(ingredient.RawMaterialId);
            var matName = rawMaterial?.Name ?? "Raw materials";
            var ingUom = rawMaterial?.UnitOfMeasure;

            var rawRequiredQty = ingredient.Quantity * multiplier;
            var requiredQty = Math.Round(rawRequiredQty, 3);

            if (ingUom == null)
            {
                items.Add(new RequirementItemDto(matName, requiredQty, 0m, false));
                continue;
            }

            var normalizedRequired = rawRequiredQty * ingUom.ConversionFactor;
            var totalAvailableNormalized = stockLots
                .Where(s => s.ItemId == ingredient.RawMaterialId && s.UnitOfMeasure.Type == ingUom.Type)
                .Sum(s => s.Quantity * s.UnitOfMeasure.ConversionFactor);

            var availableInIngredientUom = ingUom.ConversionFactor > 0
                ? totalAvailableNormalized / ingUom.ConversionFactor
                : 0m;

            var roundedAvailable = Math.Round(availableInIngredientUom, 3);
            var isSufficient = (totalAvailableNormalized + 0.000001m) >= normalizedRequired;

            items.Add(new RequirementItemDto(matName, requiredQty, roundedAvailable, isSufficient));
        }

        return Result.Success(items);
    }
}