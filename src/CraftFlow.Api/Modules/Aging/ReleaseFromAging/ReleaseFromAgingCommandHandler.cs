using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.ReleaseFromAging;
public sealed class ReleaseFromAgingCommandHandler : IRequestHandler<ReleaseFromAgingCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public ReleaseFromAgingCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ReleaseFromAgingCommand request, CancellationToken cancellationToken)
    {
        var lot = await _dbContext.Set<AgingLot>()
            .FirstOrDefaultAsync(l => l.Id == request.AgingLotId, cancellationToken);

        if (lot is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        lot.RegisterWeightLoss(request.ActualFinalQuantity);
        lot.Release();

        var stockLot = StockLot.Create(
            request.TargetWarehouseId,
            lot.ProductId,
            lot.CurrentQuantity,
            request.UnitPrice,
            lot.BatchNumber
        );

        await _dbContext.Set<StockLot>().AddAsync(stockLot, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
