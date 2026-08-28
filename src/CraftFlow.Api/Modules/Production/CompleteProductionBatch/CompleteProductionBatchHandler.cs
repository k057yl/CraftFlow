using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.CompleteProductionBatch
{
    public class CompleteProductionBatchHandler : IRequestHandler<CompleteProductionBatchCommand, Result<Guid>>
    {
        private readonly AppDbContext _dbContext;

        public CompleteProductionBatchHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(CompleteProductionBatchCommand request, CancellationToken cancellationToken)
        {
            var batch = await _dbContext.ProductionBatches
                .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

            if (batch == null)
            {
                return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Production.BATCH_NOT_FOUND));
            }

            batch.Complete(request.ActualOutputQuantity);

            var finishedProductLot = StockLot.Create(
                batch.WarehouseId,
                batch.TargetProductId,
                request.ActualOutputQuantity,
                $"BATCH-{batch.Id.ToString()[..8].ToUpper()}"
            );

            _dbContext.StockLots.Add(finishedProductLot);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(batch.Id);
        }
    }
}
