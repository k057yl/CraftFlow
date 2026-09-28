using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.DeleteDraftBatch;

public sealed class DeleteDraftBatchCommandHandler : IRequestHandler<DeleteDraftBatchCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DeleteDraftBatchCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteDraftBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.ProductionBatches
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch == null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.Production.BATCH_NOT_FOUND));
        }

        if (batch.State != BatchState.Draft)
        {
            return Result.Failure(Error.Validation(ErrorCodes.Production.BATCH_CANNOT_BE_DELETED));
        }

        _dbContext.ProductionBatches.Remove(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}