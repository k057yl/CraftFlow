using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Catalog.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.Domain;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.ReleaseFromAging;

public sealed class ReleaseFromAgingCommandHandler : IRequestHandler<ReleaseFromAgingCommand, Result>
{
    private const int MaxBatchNumberLength = 85;

    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ReleaseFromAgingCommandHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(ReleaseFromAgingCommand request, CancellationToken cancellationToken)
    {
        if (request.UnitsCount <= 0)
        {
            return Result.Failure(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));
        }

        var finalBatchNumber = !string.IsNullOrWhiteSpace(request.CustomBatchNumber)
            ? request.CustomBatchNumber.Trim()
            : string.Empty;

        if (finalBatchNumber.Length > MaxBatchNumberLength)
        {
            return Result.Failure(Error.Validation(ErrorCodes.Aging.BATCH_NUMBER_MAX_LENGTH_EXCEEDED));
        }

        var lot = await _dbContext.AgingLots
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.AgingLotId, cancellationToken);

        if (lot is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        if (string.IsNullOrWhiteSpace(finalBatchNumber))
        {
            finalBatchNumber = lot.BatchNumber.Trim();
            if (finalBatchNumber.Length > MaxBatchNumberLength)
            {
                return Result.Failure(Error.Validation(ErrorCodes.Aging.BATCH_NUMBER_MAX_LENGTH_EXCEEDED));
            }
        }

        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.UnitOfMeasure)
            .FirstOrDefaultAsync(p => p.Id == lot.ProductId, cancellationToken);

        if (product is null || product.UnitOfMeasure is null)
        {
            return Result.Failure(Error.NotFound("PRODUCT_OR_UOM_NOT_FOUND"));
        }

        if (request.DiscardedItemIds != null && request.DiscardedItemIds.Count > 0)
        {
            lot.DiscardSpecificItems(request.DiscardedItemIds, ErrorCodes.Aging.DISCARD_REASON_MANUAL_DISCARD);
        }

        if (request.HeadWeights != null && request.HeadWeights.Count > 0)
        {
            lot.UpdateHeadWeights(request.HeadWeights);
        }

        var releasedItems = lot.Items
            .Where(i => i.State == AgingItemState.IN_CHAMBER)
            .ToList();

        decimal calculatedTotalWeight = releasedItems.Count > 0
            ? releasedItems.Sum(i => i.CurrentWeight)
            : (lot.CurrentQuantity > 0 ? lot.CurrentQuantity : request.ActualFinalQuantity);

        if (calculatedTotalWeight <= 0)
        {
            return Result.Failure(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));
        }

        if (lot.StorageLocationId.HasValue)
        {
            var oldLocation = await _dbContext.StorageLocations
                .FirstOrDefaultAsync(s => s.Id == lot.StorageLocationId.Value, cancellationToken);

            if (oldLocation != null)
            {
                oldLocation.AddVolume(-lot.InitialQuantity);
            }
        }

        lot.Release();

        var batch = await _dbContext.Set<ProductionBatch>()
            .FirstOrDefaultAsync(b => b.Id == lot.ProductionBatchId, cancellationToken);

        if (batch != null)
        {
            batch.ReleaseFromAging(calculatedTotalWeight);
        }

        int finalUnitsCount = releasedItems.Count > 0
            ? releasedItems.Count
            : (product.UnitOfMeasure.Type == UnitType.Piece ? (int)Math.Round(calculatedTotalWeight) : request.UnitsCount);

        var stockLot = StockLot.Create(
            warehouseId: request.TargetWarehouseId,
            itemId: lot.ProductId,
            unitOfMeasureId: product.UnitOfMeasureId,
            initialQuantity: calculatedTotalWeight,
            unitsCount: finalUnitsCount,
            unitPrice: request.UnitPrice,
            batchNumber: finalBatchNumber,
            tenantId: _tenantContext.TenantId,
            productionBatchId: lot.ProductionBatchId
        );

        if (releasedItems.Count > 0)
        {
            stockLot.PopulateItemsFromWeights(releasedItems.Select(i => i.CurrentWeight).ToList());
        }
        else if (request.HeadWeights != null && request.HeadWeights.Count > 0)
        {
            stockLot.PopulateItemsFromWeights(request.HeadWeights);
        }

        if (request.StorageLocationIds != null && request.StorageLocationIds.Count > 0)
        {
            var targetLocations = await _dbContext.StorageLocations
                .Where(s => request.StorageLocationIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            decimal qtyPerLocation = calculatedTotalWeight / targetLocations.Count;

            foreach (var loc in targetLocations)
            {
                loc.AddVolume(qtyPerLocation);
            }
        }

        await _dbContext.Set<StockLot>().AddAsync(stockLot, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}