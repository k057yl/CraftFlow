using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Domain;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Events;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.CompleteProductionBatch;

public class CompleteProductionBatchHandler : IRequestHandler<CompleteProductionBatchCommand, Result<Guid>>
{
    private const int MaxBatchNumberLength = 85;

    private readonly AppDbContext _dbContext;
    private readonly IPublisher _publisher;

    public CompleteProductionBatchHandler(AppDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(CompleteProductionBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.ProductionBatches
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch is null)
        {
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Production.BATCH_NOT_FOUND));
        }

        if (batch.State == BatchState.DRAFT)
        {
            return Result.Failure<Guid>(Error.Validation(ErrorCodes.Production.BATCH_CANNOT_BE_DELETED));
        }

        if (!string.IsNullOrWhiteSpace(request.BatchNumber))
        {
            var trimmedName = request.BatchNumber.Trim();
            if (trimmedName.Length > MaxBatchNumberLength)
            {
                return Result.Failure<Guid>(Error.Validation(ErrorCodes.Aging.BATCH_NUMBER_MAX_LENGTH_EXCEEDED));
            }

            batch.UpdateName(trimmedName);
        }

        var validWeights = request.HeadWeights?.Where(w => w > 0).ToList();
        int unitsCount = validWeights != null && validWeights.Count > 0
            ? validWeights.Count
            : (request.UnitsCount > 0 ? request.UnitsCount : 1);

        decimal actualOutput = validWeights != null && validWeights.Count > 0
            ? validWeights.Sum()
            : request.ActualOutputQuantity;

        if (batch.EquipmentLocationId != Guid.Empty)
        {
            var equipmentLocation = await _dbContext.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == batch.EquipmentLocationId, cancellationToken);

            if (equipmentLocation != null)
            {
                equipmentLocation.ReleaseFromProduction();
            }
        }

        if (request.RequiresAging && request.AgingChamberId.HasValue)
        {
            batch.MarkReadyForAging(actualOutput, unitsCount, validWeights);
            batch.MarkAsTransferredToAging();

            var agingLot = AgingLot.Create(
                productionBatchId: batch.Id,
                productId: batch.TargetProductId,
                agingChamberId: request.AgingChamberId.Value,
                batchNumber: batch.Name,
                initialQuantity: actualOutput,
                unitsCount: unitsCount,
                minAgingDays: request.MinAgingDays ?? 0,
                storageLocationId: request.StorageLocationId,
                headWeights: validWeights
            );

            await _dbContext.Set<AgingLot>().AddAsync(agingLot, cancellationToken);
        }
        else
        {
            batch.Complete(
                actualOutputQuantity: actualOutput,
                unitsCount: unitsCount,
                overheadPercentage: request.OverheadPercentage,
                headWeights: validWeights
            );
        }

        await _publisher.Publish(
            new ProductionBatchCompletedEvent(batch.Id, batch.RecipeId, batch.DestinationWarehouseId, batch.ActualOutputQuantity, unitsCount, validWeights),
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(batch.Id);
    }
}