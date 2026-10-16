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
            .Where(s => s.WarehouseId == request.WarehouseId && rawMaterialIds.Contains(s.ItemId) && s.Quantity > 0)
            .ToListAsync(cancellationToken);

        var multiplier = request.PlannedQty / recipe.TargetOutputQuantity;
        var items = new List<RequirementItemDto>();

        foreach (var ingredient in recipe.Ingredients)
        {
            var rawMaterial = rawMaterials.GetValueOrDefault(ingredient.RawMaterialId);
            var matName = rawMaterial?.Name ?? "Raw materials";
            decimal conversionFactor = rawMaterial?.UnitOfMeasure?.ConversionFactor ?? 1.0m;
            if (conversionFactor <= 0m) conversionFactor = 1.0m;

            var requiredInBaseUnits = (ingredient.Quantity * multiplier) * conversionFactor;
            var requiredQty = Math.Round(requiredInBaseUnits, 4);
            var totalAvailable = stockLots
                .Where(s => s.ItemId == ingredient.RawMaterialId)
                .Sum(s => s.Quantity);

            var roundedAvailable = Math.Round(totalAvailable, 3);
            var isSufficient = totalAvailable >= requiredQty;

            items.Add(new RequirementItemDto(matName, requiredQty, roundedAvailable, isSufficient));
        }

        return Result.Success(items);
    }
}