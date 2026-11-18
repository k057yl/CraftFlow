using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.DeleteDraftBatch;

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

        if (batch.State != BatchState.DRAFT)
        {
            return Result.Failure(Error.Validation(ErrorCodes.Production.BATCH_CANNOT_BE_DELETED));
        }

        if (batch.EquipmentLocationId != Guid.Empty)
        {
            var equipmentLocation = await _dbContext.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == batch.EquipmentLocationId, cancellationToken);

            if (equipmentLocation != null)
            {
                equipmentLocation.CompleteSanitation(isPassed: true);
            }
        }

        _dbContext.ProductionBatches.Remove(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}