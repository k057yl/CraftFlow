using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.DiscardSpecificAgingItems;
public class DiscardSpecificAgingItemsHandler : IRequestHandler<DiscardSpecificAgingItemsCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DiscardSpecificAgingItemsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DiscardSpecificAgingItemsCommand request, CancellationToken cancellationToken)
    {
        if (request.ItemIds is null || request.ItemIds.Count == 0)
        {
            return Result.Failure(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));
        }

        var lot = await _dbContext.AgingLots
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.AgingLotId, cancellationToken);

        if (lot is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        lot.DiscardSpecificItems(request.ItemIds, request.Reason);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}