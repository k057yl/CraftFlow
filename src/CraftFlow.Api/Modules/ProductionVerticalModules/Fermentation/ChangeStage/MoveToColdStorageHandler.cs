using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.ChangeStage;
public sealed class MoveToColdStorageHandler : IRequestHandler<MoveToColdStorageCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public MoveToColdStorageHandler(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result> Handle(MoveToColdStorageCommand request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FermentationBatches
            .FirstOrDefaultAsync(b => b.Id == request.FermentationBatchId, cancellationToken);

        if (batch is null)
            return Result.Failure(new Error(ErrorCodes.Fermentation.BATCH_NOT_FOUND));

        batch.MoveToColdStorage();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}