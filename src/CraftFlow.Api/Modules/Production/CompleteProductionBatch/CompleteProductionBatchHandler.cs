using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Events;
using CraftFlow.SharedKernel.Constants;
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
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch == null)
        {
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Production.BATCH_NOT_FOUND));
        }

        if (!string.IsNullOrWhiteSpace(request.BatchNumber))
        {
            batch.UpdateName(request.BatchNumber);
        }

        batch.Complete(request.ActualOutputQuantity);

        await _publisher.Publish(
            new ProductionBatchCompletedEvent(
                batch.Id,
                batch.RecipeId,
                batch.WarehouseId,
                batch.ActualOutputQuantity,
                request.UnitsCount
            ),
            cancellationToken
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(batch.Id);
    }
}