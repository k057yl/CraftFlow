using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.EstimateCost;

public class EstimateCostHandler : IRequestHandler<EstimateCostQuery, Result<decimal>>
{
    private readonly AppDbContext _dbContext;

    public EstimateCostHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<decimal>> Handle(EstimateCostQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);

        if (recipe == null || recipe.TargetOutputQuantity <= 0 || recipe.Ingredients == null || !recipe.Ingredients.Any())
        {
            return Result.Success(0m);
        }

        var multiplier = request.PlannedQty / recipe.TargetOutputQuantity;
        decimal totalEstimatedCost = 0m;

        foreach (var ingredient in recipe.Ingredients)
        {
            var requiredQty = ingredient.Quantity * multiplier;

            var activeLots = await _dbContext.StockLots
                .AsNoTracking()
                .Where(s => s.ItemId == ingredient.RawMaterialId && s.Quantity > 0)
                .Select(s => new { s.Quantity, s.UnitPrice })
                .ToListAsync(cancellationToken);

            var totalQuantityOnStock = activeLots.Sum(l => l.Quantity);
            decimal weightedAvgUnitPrice = 0m;

            if (totalQuantityOnStock > 0)
            {
                var totalStockCost = activeLots.Sum(l => l.Quantity * l.UnitPrice);
                weightedAvgUnitPrice = totalStockCost / totalQuantityOnStock;
            }

            totalEstimatedCost += requiredQty * weightedAvgUnitPrice;
        }

        return Result.Success(totalEstimatedCost);
    }
}