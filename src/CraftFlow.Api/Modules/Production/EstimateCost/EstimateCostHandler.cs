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

        var rawMaterialIds = recipe.Ingredients.Select(i => i.RawMaterialId).ToList();
        var rawMaterials = await _dbContext.RawMaterials
            .AsNoTracking()
            .Include(rm => rm.UnitOfMeasure)
            .Where(rm => rawMaterialIds.Contains(rm.Id))
            .ToDictionaryAsync(rm => rm.Id, cancellationToken);

        var multiplier = request.PlannedQty / recipe.TargetOutputQuantity;
        decimal totalEstimatedCost = 0m;

        foreach (var ingredient in recipe.Ingredients)
        {
            var rawMaterial = rawMaterials.GetValueOrDefault(ingredient.RawMaterialId);
            if (rawMaterial?.UnitOfMeasure == null) continue;

            var ingUom = rawMaterial.UnitOfMeasure;
            var requiredQty = ingredient.Quantity * multiplier;
            var normalizedRequiredQty = requiredQty * ingUom.ConversionFactor;

            var activeLots = await _dbContext.StockLots
                .AsNoTracking()
                .Include(s => s.UnitOfMeasure)
                .Where(s => s.ItemId == ingredient.RawMaterialId && s.Quantity > 0)
                .ToListAsync(cancellationToken);

            var validLots = activeLots.Where(l => l.UnitOfMeasure.Type == ingUom.Type).ToList();

            decimal totalStockCost = 0m;
            decimal totalNormalizedStockQuantity = 0m;

            foreach (var lot in validLots)
            {
                var normalizedLotQty = lot.Quantity * lot.UnitOfMeasure.ConversionFactor;

                totalStockCost += lot.Quantity * lot.UnitPrice;
                totalNormalizedStockQuantity += normalizedLotQty;
            }

            if (totalNormalizedStockQuantity > 0)
            {
                var costPerBaseUnit = totalStockCost / totalNormalizedStockQuantity;
                totalEstimatedCost += normalizedRequiredQty * costPerBaseUnit;
            }
        }

        return Result.Success(Math.Round(totalEstimatedCost, 2));
    }
}