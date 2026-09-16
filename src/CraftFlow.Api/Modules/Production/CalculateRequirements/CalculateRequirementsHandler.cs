using CraftFlow.Api.Common.Persistence;
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
            .Where(rm => rawMaterialIds.Contains(rm.Id))
            .ToDictionaryAsync(rm => rm.Id, rm => rm.Name, cancellationToken);

        var multiplier = request.PlannedQty / recipe.TargetOutputQuantity;
        var items = new List<RequirementItemDto>();

        foreach (var ingredient in recipe.Ingredients)
        {
            var requiredQty = Math.Round(ingredient.Quantity * multiplier, 3);

            var availableQty = await _dbContext.StockLots
                .AsNoTracking()
                .Where(s => s.WarehouseId == request.WarehouseId && s.ItemId == ingredient.RawMaterialId && s.Quantity > 0)
                .SumAsync(s => (decimal?)s.Quantity, cancellationToken) ?? 0m;

            var matName = rawMaterials.TryGetValue(ingredient.RawMaterialId, out var name) ? name : "Raw materials";
            var roundedAvailable = Math.Round(availableQty, 3);
            var isSufficient = (roundedAvailable + 0.001m) >= requiredQty;

            items.Add(new RequirementItemDto(matName, requiredQty, roundedAvailable, isSufficient));
        }

        return Result.Success(items);
    }
}