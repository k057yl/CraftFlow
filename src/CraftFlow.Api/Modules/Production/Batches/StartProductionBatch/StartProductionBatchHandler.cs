using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Modules.Production.Batches.StartProductionBatch;

public class StartProductionBatchHandler : IRequestHandler<StartProductionBatchCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    public StartProductionBatchHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(StartProductionBatchCommand request, CancellationToken cancellationToken)
    {
        var equipmentLocation = await _dbContext.StorageLocations
            .FirstOrDefaultAsync(l => l.Id == request.EquipmentLocationId, cancellationToken);

        if (equipmentLocation == null)
        {
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Inventory.LOCATION_NOT_FOUND));
        }

        if (equipmentLocation.Status != StorageLocationStatus.AVAILABLE)
        {
            return Result.Failure<Guid>(Error.Validation(ErrorCodes.Inventory.LOCATION_NOT_AVAILABLE));
        }

        var recipe = await _dbContext.Recipes
            .Include(r => r.Ingredients)
            .FirstAsync(r => r.Id == request.RecipeId, cancellationToken);

        var rawMaterialIds = recipe.Ingredients.Select(i => i.RawMaterialId).ToList();

        var rawMaterials = await _dbContext.RawMaterials
            .Include(rm => rm.UnitOfMeasure)
            .Where(rm => rawMaterialIds.Contains(rm.Id))
            .ToDictionaryAsync(rm => rm.Id, cancellationToken);

        var multiplier = request.PlannedOutputQuantity / recipe.TargetOutputQuantity;

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
            var rawMaterial = rawMaterials.GetValueOrDefault(recipeIngredient.RawMaterialId);
            decimal conversionFactor = rawMaterial?.UnitOfMeasure?.ConversionFactor ?? 1.0m;
            if (conversionFactor <= 0m) conversionFactor = 1.0m;

            var requiredQuantity = recipeIngredient.Quantity * multiplier * conversionFactor;

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

        equipmentLocation.AddVolume(request.PlannedOutputQuantity);

        _dbContext.ProductionBatches.Add(batch);

        foreach (var consumed in consumedIngredientsToSave)
        {
            var finalConsumed = ConsumedIngredient.Create(batch.Id, consumed.StockLotId, consumed.RawMaterialId, consumed.Quantity, batch.TenantId);
            _dbContext.Set<ConsumedIngredient>().Add(finalConsumed);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var batchQuotaCacheKey = $"quota:batches:{_tenantContext.TenantId}:{DateTime.UtcNow:yyyyMM}";
        _cache.Remove(batchQuotaCacheKey);

        return Result.Success(batch.Id);
    }
}