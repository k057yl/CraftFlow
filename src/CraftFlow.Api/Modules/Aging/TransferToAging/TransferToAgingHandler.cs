using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.TransferToAging;

public sealed class TransferToAgingHandler : IRequestHandler<TransferToAgingCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public TransferToAgingHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(TransferToAgingCommand request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.Set<ProductionBatch>()
            .FirstAsync(b => b.Id == request.ProductionBatchId, cancellationToken);

        var finalLotNumber = !string.IsNullOrWhiteSpace(request.CustomBatchNumber)
            ? request.CustomBatchNumber.Trim()
            : (!string.IsNullOrWhiteSpace(batch.Name)
                ? batch.Name
                : string.Format(FormattingConstants.BATCH_NUMBER_FORMAT, DateTime.UtcNow, batch.Id.ToString()[..4].ToUpperInvariant()));

        var initialQuantity = batch.ActualOutputQuantity > 0
            ? batch.ActualOutputQuantity
            : batch.PlannedOutputQuantity;

        int unitsCount = request.UnitsCount > 0 ? request.UnitsCount : 1;

        var agingLot = AgingLot.Create(
            batch.Id,
            batch.TargetProductId,
            request.AgingChamberId,
            finalLotNumber,
            initialQuantity,
            unitsCount,
            request.MinAgingDays
        );

        batch.MarkAsTransferredToAging();

        await _dbContext.Set<AgingLot>().AddAsync(agingLot, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(agingLot.Id);
    }
}