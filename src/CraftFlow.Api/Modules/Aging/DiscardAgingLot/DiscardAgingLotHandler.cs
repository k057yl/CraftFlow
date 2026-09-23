using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.DiscardAgingLot;

public class DiscardAgingLotHandler : IRequestHandler<DiscardAgingLotCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DiscardAgingLotHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DiscardAgingLotCommand request, CancellationToken cancellationToken)
    {
        var lot = await _dbContext.AgingLots
            .FirstOrDefaultAsync(l => l.Id == request.AgingLotId, cancellationToken);

        if (lot is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        int unitsToRemove = request.UnitsToRemove > 0 ? request.UnitsToRemove : 1;
        int newUnitsCount = lot.UnitsCount - unitsToRemove;
        decimal newQuantity = lot.CurrentQuantity - request.Quantity;

        if (newUnitsCount <= 0 || newQuantity <= 0)
        {
            lot.Discard();
        }
        else
        {
            lot.RegisterLoss(newQuantity, newUnitsCount);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}