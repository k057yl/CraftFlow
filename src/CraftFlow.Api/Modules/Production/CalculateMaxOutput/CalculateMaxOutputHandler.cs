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

        decimal maxPossibleMultiplier = decimal.MaxValue;

        foreach (var ingredient in recipe.Ingredients)
        {
            if (ingredient.Quantity <= 0) continue;

            if (!rawMaterials.TryGetValue(ingredient.RawMaterialId, out var rawMaterial) || rawMaterial.UnitOfMeasure == null)
            {
                return Result.Success(0m);
            }

            var ingUom = rawMaterial.UnitOfMeasure;
            var normalizedReqPerBatch = ingredient.Quantity * ingUom.ConversionFactor;
            var totalAvailableNormalized = stockLots
                .Where(s => s.ItemId == ingredient.RawMaterialId && s.UnitOfMeasure.Type == ingUom.Type)
                .Sum(s => s.Quantity * s.UnitOfMeasure.ConversionFactor);

            if (totalAvailableNormalized <= 0m || normalizedReqPerBatch <= 0m)
            {
                return Result.Success(0m);
            }

            var ingredientLimitMultiplier = totalAvailableNormalized / normalizedReqPerBatch;

            if (ingredientLimitMultiplier < maxPossibleMultiplier)
            {
                maxPossibleMultiplier = ingredientLimitMultiplier;
            }
        }

        if (maxPossibleMultiplier == decimal.MaxValue)
        {
            return Result.Success(0m);
        }

        var maxPlannedOutput = Math.Floor(recipe.TargetOutputQuantity * maxPossibleMultiplier * 100m) / 100m;
        return Result.Success(maxPlannedOutput);
    }
}