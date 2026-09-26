using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Production.Events;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.CompleteProductionBatch;

public class CompleteProductionBatchHandler : IRequestHandler<CompleteProductionBatchCommand, Result<Guid>>
{
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
            .FirstAsync(b => b.Id == request.BatchId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.BatchNumber))
        {
            batch.UpdateName(request.BatchNumber);
        }

        int unitsCount = request.UnitsCount > 0 ? request.UnitsCount : 1;

        if (request.RequiresAging && request.AgingChamberId.HasValue)
        {
            batch.MarkReadyForAging(request.ActualOutputQuantity, unitsCount);
            batch.MarkAsTransferredToAging();

            var agingLot = AgingLot.Create(
                productionBatchId: batch.Id,
                productId: batch.TargetProductId,
                agingChamberId: request.AgingChamberId.Value,
                batchNumber: batch.Name,
                initialQuantity: request.ActualOutputQuantity,
                unitsCount: unitsCount,
                minAgingDays: request.MinAgingDays ?? 0,
                storageLocationId: request.StorageLocationId
            );

            await _dbContext.Set<AgingLot>().AddAsync(agingLot, cancellationToken);
        }
        else
        {
            batch.Complete(
                actualOutputQuantity: request.ActualOutputQuantity,
                unitsCount: unitsCount,
                overheadPercentage: request.OverheadPercentage
            );
        }

        await _publisher.Publish(
            new ProductionBatchCompletedEvent(batch.Id, batch.RecipeId, batch.WarehouseId, batch.ActualOutputQuantity, unitsCount),
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(batch.Id);
    }
}