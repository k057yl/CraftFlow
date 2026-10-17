using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.Planning.CalculateRequirements;

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
                .ThenInclude(i => i.RawMaterial)
                    .ThenInclude(rm => rm.UnitOfMeasure)
            .FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);

        if (recipe == null || recipe.TargetOutputQuantity <= 0 || recipe.Ingredients == null || !recipe.Ingredients.Any())
        {
            return Result.Success(new List<RequirementItemDto>());
        }

        var rawMaterialIds = recipe.Ingredients.Select(i => i.RawMaterialId).ToList();

        var stockLots = await _dbContext.StockLots
            .AsNoTracking()
            .Where(s => s.WarehouseId == request.WarehouseId && rawMaterialIds.Contains(s.ItemId) && s.Quantity > 0)
            .ToListAsync(cancellationToken);

        var multiplier = request.PlannedQty / recipe.TargetOutputQuantity;
        var items = new List<RequirementItemDto>();

        foreach (var ingredient in recipe.Ingredients)
        {
            var rawMaterial = ingredient.RawMaterial;
            var matName = rawMaterial?.Name ?? "Raw materials";

            decimal conversionFactor = rawMaterial?.UnitOfMeasure?.ConversionFactor ?? 1.0m;
            if (conversionFactor <= 0m) conversionFactor = 1.0m;

            decimal requiredInBaseUnits = ingredient.Quantity * multiplier * conversionFactor;
            decimal requiredQty = Math.Round(requiredInBaseUnits, 4);

            decimal totalAvailable = stockLots
                .Where(s => s.ItemId == ingredient.RawMaterialId)
                .Sum(s => s.Quantity);

            decimal roundedAvailable = Math.Round(totalAvailable, 4);
            bool isSufficient = totalAvailable >= requiredQty;

            items.Add(new RequirementItemDto(matName, requiredQty, roundedAvailable, isSufficient));
        }

        return Result.Success(items);
    }
}