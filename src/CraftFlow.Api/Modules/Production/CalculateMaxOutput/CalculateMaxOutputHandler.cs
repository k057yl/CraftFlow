using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.CalculateMaxOutput;

public class CalculateMaxOutputHandler : IRequestHandler<CalculateMaxOutputQuery, Result<decimal>>
{
    private readonly AppDbContext _dbContext;

    public CalculateMaxOutputHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<decimal>> Handle(CalculateMaxOutputQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);

        if (recipe == null || recipe.TargetOutputQuantity <= 0 || recipe.Ingredients == null || !recipe.Ingredients.Any())
        {
            return Result.Success(0m);
        }

        var rawMaterialIds = recipe.Ingredients.Select(i => i.RawMaterialId).ToList();
        var stockSummary = await _dbContext.StockLots
            .AsNoTracking()
            .Where(s => s.WarehouseId == request.WarehouseId && rawMaterialIds.Contains(s.ItemId) && s.Quantity > 0)
            .GroupBy(s => s.ItemId)
            .Select(g => new { ItemId = g.Key, TotalQuantity = g.Sum(s => s.Quantity) })
            .ToDictionaryAsync(g => g.ItemId, g => g.TotalQuantity, cancellationToken);

        decimal maxPossibleMultiplier = decimal.MaxValue;

        foreach (var ingredient in recipe.Ingredients)
        {
            if (ingredient.Quantity <= 0) continue;

            stockSummary.TryGetValue(ingredient.RawMaterialId, out var availableStock);

            if (availableStock <= 0)
            {
                return Result.Success(0m);
            }

            var ingredientLimitMultiplier = availableStock / ingredient.Quantity;

            if (ingredientLimitMultiplier < maxPossibleMultiplier)
            {
                maxPossibleMultiplier = ingredientLimitMultiplier;
            }
        }

        var maxPlannedOutput = Math.Floor(recipe.TargetOutputQuantity * maxPossibleMultiplier * 100m) / 100m;
        return Result.Success(maxPlannedOutput);
    }
}