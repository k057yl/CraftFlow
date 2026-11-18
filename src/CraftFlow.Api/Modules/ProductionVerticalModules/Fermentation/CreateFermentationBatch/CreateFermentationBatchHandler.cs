using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.CreateFermentationBatch;

public sealed class CreateFermentationBatchHandler : IRequestHandler<CreateFermentationBatchCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    public CreateFermentationBatchHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateFermentationBatchCommand request, CancellationToken cancellationToken)
    {
        var equipmentLocation = await _dbContext.StorageLocations
            .FirstOrDefaultAsync(l => l.Id == request.EquipmentLocationId, cancellationToken);

        if (equipmentLocation is null)
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Inventory.LOCATION_NOT_FOUND));

        if (equipmentLocation.Status != StorageLocationStatus.AVAILABLE)
            return Result.Failure<Guid>(Error.Validation(ErrorCodes.Inventory.LOCATION_NOT_AVAILABLE));

        var recipe = await _dbContext.Recipes
            .IgnoreQueryFilters()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);

        if (recipe is null)
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Catalog.RECIPE_NOT_FOUND));

        var rawMaterialIds = recipe.Ingredients.Select(i => i.RawMaterialId).ToList();

        var rawMaterials = await _dbContext.RawMaterials
            .Include(rm => rm.UnitOfMeasure)
            .Where(rm => rawMaterialIds.Contains(rm.Id))
            .ToDictionaryAsync(rm => rm.Id, cancellationToken);

        var multiplier = request.SubstrateWeightKg / recipe.TargetOutputQuantity;

        var allStockLots = await _dbContext.StockLots
            .Include(s => s.StorageLocations)
            .Where(s => s.WarehouseId == request.WarehouseId && rawMaterialIds.Contains(s.ItemId) && s.Quantity > 0)
            .OrderBy(s => s.CreatedDate)
            .ToListAsync(cancellationToken);

        var allLocationIds = allStockLots
            .SelectMany(sl => sl.StorageLocations.Select(l => l.StorageLocationId))
            .Distinct()
            .ToList();

        var allLocations = await _dbContext.StorageLocations
            .Where(sl => allLocationIds.Contains(sl.Id))
            .ToListAsync(cancellationToken);

        var batchNumber = !string.IsNullOrWhiteSpace(request.BatchName)
            ? request.BatchName
            : $"{FormattingConstants.BATCH_PREFIX}{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";

        var batch = FermentationBatch.Create(
            tenantId: _tenantContext.TenantId,
            recipeId: recipe.Id,
            targetProductId: recipe.ProductId,
            warehouseId: request.WarehouseId,
            destinationWarehouseId: request.DestinationWarehouseId,
            equipmentLocationId: request.EquipmentLocationId,
            batchNumber: batchNumber,
            recipeName: recipe.Name,
            substrateWeightKg: request.SubstrateWeightKg,
            saltPercentage: request.SaltPercentage,
            targetTemperature: request.TargetTemperature,
            vesselInfo: equipmentLocation.Name,
            initialPh: request.InitialPh
        );

        foreach (var recipeIngredient in recipe.Ingredients)
        {
            var rawMaterial = rawMaterials.GetValueOrDefault(recipeIngredient.RawMaterialId);
            decimal conversionFactor = rawMaterial?.UnitOfMeasure?.ConversionFactor ?? 1.0m;
            if (conversionFactor <= 0m) conversionFactor = 1.0m;

            var requiredQuantity = recipeIngredient.Quantity * multiplier * conversionFactor;

            var availableLots = allStockLots.Where(s => s.ItemId == recipeIngredient.RawMaterialId).ToList();
            var totalAvailable = availableLots.Sum(s => s.Quantity);

            if (totalAvailable < requiredQuantity)
                return Result.Failure<Guid>(Error.Validation(ErrorCodes.Production.INSUFFICIENT_RAW_MATERIAL));

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

                batch.AddConsumedIngredient(lot.Id, recipeIngredient.RawMaterialId, deduct);
            }
        }

        equipmentLocation.LockForProduction(batch.Id);

        await _dbContext.FermentationBatches.AddAsync(batch, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _cache.Remove($"quota:batches:{_tenantContext.TenantId}:{DateTime.UtcNow:yyyyMM}");

        return Result.Success(batch.Id);
    }
}