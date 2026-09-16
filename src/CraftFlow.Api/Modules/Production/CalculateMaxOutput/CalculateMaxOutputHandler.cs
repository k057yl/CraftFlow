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

        decimal maxPossibleMultiplier = decimal.MaxValue;

        foreach (var ingredient in recipe.Ingredients)
        {
            if (ingredient.Quantity <= 0) continue;

            var availableStock = await _dbContext.StockLots
                .AsNoTracking()
                .Where(s => s.WarehouseId == request.WarehouseId && s.ItemId == ingredient.RawMaterialId && s.Quantity > 0)
                .SumAsync(s => s.Quantity, cancellationToken);

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

        if (maxPossibleMultiplier == decimal.MaxValue || maxPossibleMultiplier <= 0)
        {
            return Result.Success(0m);
        }

        var maxPlannedOutput = Math.Floor(recipe.TargetOutputQuantity * maxPossibleMultiplier * 100m) / 100m;

        return Result.Success(maxPlannedOutput);
    }
}