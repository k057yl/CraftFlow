using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.DiscardFermentationBatch;

public sealed class DiscardFermentationBatchHandler : IRequestHandler<DiscardFermentationBatchCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DiscardFermentationBatchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DiscardFermentationBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FermentationBatches
            .FirstOrDefaultAsync(fb => fb.Id == request.FermentationBatchId, cancellationToken);

        if (batch is null)
            return Result.Failure(new Error(ErrorCodes.Fermentation.BATCH_NOT_FOUND));

        batch.Discard(request.Reason);

        if (batch.EquipmentLocationId != Guid.Empty)
        {
            var equipmentLocation = await _dbContext.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == batch.EquipmentLocationId, cancellationToken);

            equipmentLocation?.ReleaseFromDiscard();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}