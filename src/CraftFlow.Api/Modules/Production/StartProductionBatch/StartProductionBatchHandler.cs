using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.StartProductionBatch;

public class StartProductionBatchHandler : IRequestHandler<StartProductionBatchCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public StartProductionBatchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(StartProductionBatchCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Recipes
            .Include(r => r.Ingredients)
            .FirstAsync(r => r.Id == request.RecipeId, cancellationToken);

        var multiplier = request.PlannedOutputQuantity / recipe.TargetOutputQuantity;
        var rawMaterialIds = recipe.Ingredients.Select(i => i.RawMaterialId).ToList();
        var allStockLots = await _dbContext.StockLots
            .Include(s => s.StorageLocations)
            .Where(s => s.WarehouseId == request.WarehouseId && rawMaterialIds.Contains(s.ItemId) && s.Quantity > 0)
            .OrderBy(s => s.CreatedDate)
            .ToListAsync(cancellationToken);

        var allLocationIds = allStockLots.SelectMany(sl => sl.StorageLocations.Select(l => l.StorageLocationId)).Distinct().ToList();
        var allLocations = await _dbContext.StorageLocations
            .Where(sl => allLocationIds.Contains(sl.Id))
            .ToListAsync(cancellationToken);

        var consumedIngredientsToSave = new List<ConsumedIngredient>();

        foreach (var recipeIngredient in recipe.Ingredients)
        {
            var requiredQuantity = recipeIngredient.Quantity * multiplier;
            var availableLots = allStockLots.Where(s => s.ItemId == recipeIngredient.RawMaterialId).ToList();
            var totalAvailable = availableLots.Sum(s => s.Quantity);

            if (totalAvailable < requiredQuantity)
            {
                return Result.Failure<Guid>(Error.Validation(ErrorCodes.Production.INSUFFICIENT_RAW_MATERIAL));
            }

            var remainingToDeduct = requiredQuantity;

            foreach (var lot in availableLots)
            {
                if (remainingToDeduct <= 0) break;

                var deduct = Math.Min(lot.Quantity, remainingToDeduct);
                lot.AdjustQuantity(-deduct);
                remainingToDeduct -= deduct;

                decimal remainingToFree = deduct;
                foreach (var stockLoc in lot.StorageLocations)
                {
                    if (remainingToFree <= 0) break;

                    var loc = allLocations.FirstOrDefault(l => l.Id == stockLoc.StorageLocationId);
                    if (loc != null)
                    {
                        decimal amountToFree = Math.Min(stockLoc.AllocatedQuantity, remainingToFree);
                        loc.AddVolume(-amountToFree);
                        remainingToFree -= amountToFree;
                    }
                }

                consumedIngredientsToSave.Add(ConsumedIngredient.Create(
                    Guid.Empty,
                    lot.Id,
                    recipeIngredient.RawMaterialId,
                    deduct,
                    Guid.Empty
                ));
            }
        }

        var destinationWarehouse = request.DestinationWarehouseId != Guid.Empty
            ? request.DestinationWarehouseId
            : request.WarehouseId;

        var batch = ProductionBatch.Create(
            request.RecipeId,
            recipe.ProductId,
            request.WarehouseId,
            destinationWarehouse,
            request.PlannedOutputQuantity,
            recipe.TargetDurationMinutes,
            request.Name
        );

        batch.Start();
        _dbContext.ProductionBatches.Add(batch);

        foreach (var consumed in consumedIngredientsToSave)
        {
            var finalConsumed = ConsumedIngredient.Create(batch.Id, consumed.StockLotId, consumed.RawMaterialId, consumed.Quantity, batch.TenantId);
            _dbContext.Set<ConsumedIngredient>().Add(finalConsumed);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(batch.Id);
    }
}