using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.CompleteFermentationBatch;

public sealed class CompleteFermentationBatchHandler : IRequestHandler<CompleteFermentationBatchCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public CompleteFermentationBatchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(CompleteFermentationBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FermentationBatches
            .Include(b => b.ConsumedIngredients)
            .FirstOrDefaultAsync(b => b.Id == request.FermentationBatchId, cancellationToken);

        if (batch is null)
            return Result.Failure(new Error(ErrorCodes.Fermentation.BATCH_NOT_FOUND));

        if (batch.EquipmentLocationId != Guid.Empty)
        {
            var equipmentLocation = await _dbContext.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == batch.EquipmentLocationId, cancellationToken);

            equipmentLocation?.ReleaseFromProduction();
        }

        batch.Complete(
            request.ActualOutputQuantity,
            request.UnitsCount,
            request.OverheadPercentage,
            request.BatchNumber
        );

        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == batch.TargetProductId, cancellationToken);

        if (product != null && request.ActualOutputQuantity > 0)
        {
            var lotIds = batch.ConsumedIngredients.Select(ci => ci.StockLotId).Distinct().ToList();
            var lotPrices = await _dbContext.StockLots
                .AsNoTracking()
                .Where(sl => lotIds.Contains(sl.Id))
                .ToDictionaryAsync(sl => sl.Id, sl => sl.UnitPrice, cancellationToken);

            decimal totalRawCost = batch.ConsumedIngredients
                .Sum(ci => ci.Quantity * lotPrices.GetValueOrDefault(ci.StockLotId, 0m));

            decimal overheadMultiplier = 1m + ((request.OverheadPercentage ?? 0m) / 100m);
            decimal unitCost = (totalRawCost * overheadMultiplier) / request.ActualOutputQuantity;

            var finishedStockLot = StockLot.Create(
                tenantId: batch.TenantId,
                warehouseId: batch.DestinationWarehouseId,
                itemId: batch.TargetProductId,
                unitOfMeasureId: product.UnitOfMeasureId,
                initialQuantity: request.ActualOutputQuantity,
                unitsCount: batch.UnitsCount,
                unitPrice: unitCost,
                batchNumber: batch.BatchNumber,
                productionBatchId: null
            );

            await _dbContext.StockLots.AddAsync(finishedStockLot, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}