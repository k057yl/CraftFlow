using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Production.Batches.CompleteProductionBatch;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.CompleteFermentationBatch;

public sealed class CompleteFermentationBatchHandler : IRequestHandler<CompleteFermentationBatchCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly ISender _sender;

    public CompleteFermentationBatchHandler(AppDbContext dbContext, ISender sender)
    {
        _dbContext = dbContext;
        _sender = sender;
    }

    public async Task<Result> Handle(CompleteFermentationBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FermentationBatches
            .FirstOrDefaultAsync(b => b.Id == request.FermentationBatchId, cancellationToken);

        if (batch is null)
            return Result.Failure(new Error(ErrorCodes.Fermentation.BATCH_NOT_FOUND));

        var completeProdResult = await _sender.Send(new CompleteProductionBatchCommand(
            BatchId: batch.ProductionBatchId,
            ActualOutputQuantity: request.ActualOutputQuantity,
            UnitsCount: request.UnitsCount,
            OverheadPercentage: request.OverheadPercentage,
            BatchNumber: request.BatchNumber,
            RequiresAging: false,
            AgingChamberId: null,
            MinAgingDays: null,
            StorageLocationId: null
        ), cancellationToken);

        if (completeProdResult.IsFailure)
            return Result.Failure(completeProdResult.Error);

        batch.Complete();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}