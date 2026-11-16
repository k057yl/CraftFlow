using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Production.Batches.DiscardBatch;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.DiscardFermentationBatch;

public sealed class DiscardFermentationBatchHandler : IRequestHandler<DiscardFermentationBatchCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly ISender _sender;

    public DiscardFermentationBatchHandler(AppDbContext dbContext, ISender sender)
    {
        _dbContext = dbContext;
        _sender = sender;
    }

    public async Task<Result> Handle(DiscardFermentationBatchCommand request, CancellationToken cancellationToken)
    {
        var fermentationBatch = await _dbContext.FermentationBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(fb => fb.Id == request.FermentationBatchId, cancellationToken);

        if (fermentationBatch is null)
        {
            return Result.Failure(new Error(ErrorCodes.Fermentation.BATCH_NOT_FOUND));
        }

        return await _sender.Send(
            new DiscardBatchCommand(fermentationBatch.ProductionBatchId, request.Reason),
            cancellationToken);
    }
}