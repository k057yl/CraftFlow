using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.DiscardBatch;

public class DiscardBatchHandler : IRequestHandler<DiscardBatchCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DiscardBatchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DiscardBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.Set<ProductionBatch>()
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.Production.BATCH_NOT_FOUND));
        }

        batch.Discard(request.Reason);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}