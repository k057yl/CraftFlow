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
            .IgnoreQueryFilters()
            .Where(l => l.Id == request.AgingLotId)
            .Include(l => l.Items)
            .FirstOrDefaultAsync(cancellationToken);

        if (lot is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        lot.DiscardUnits(request.Quantity, request.UnitsToRemove, request.Reason);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}