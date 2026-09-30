using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.ReleaseFromAging;

public sealed class ReleaseFromAgingCommandHandler : IRequestHandler<ReleaseFromAgingCommand, Result>
{
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

        var lot = await _dbContext.AgingLots
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.AgingLotId, cancellationToken);

        if (lot is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        if (request.HeadWeights != null && request.HeadWeights.Count > 0)
        {
            lot.UpdateHeadWeights(request.HeadWeights);
        }

        decimal calculatedTotalWeight = lot.CurrentQuantity > 0
            ? lot.CurrentQuantity
            : request.ActualFinalQuantity;

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
                oldLocation.AddVolume(-lot.CurrentQuantity);
            }
        }

        lot.Release();

        var batch = await _dbContext.Set<ProductionBatch>()
            .FirstOrDefaultAsync(b => b.Id == lot.ProductionBatchId, cancellationToken);

        if (batch != null)
        {
            batch.ReleaseFromAging(calculatedTotalWeight);
        }

        var cleanBatchName = lot.BatchNumber.Contains("(Выход:")
            ? lot.BatchNumber.Substring(0, lot.BatchNumber.IndexOf("(Выход:")).Trim()
            : lot.BatchNumber;

        var finalBatchNumber = $"{cleanBatchName} (Выход: {calculatedTotalWeight:N2} кг)";
        var stockLot = StockLot.Create(
            warehouseId: request.TargetWarehouseId,
            itemId: lot.ProductId,
            initialQuantity: calculatedTotalWeight,
            unitsCount: request.UnitsCount,
            unitPrice: request.UnitPrice,
            batchNumber: finalBatchNumber,
            tenantId: _tenantContext.TenantId,
            productionBatchId: lot.ProductionBatchId
        );

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