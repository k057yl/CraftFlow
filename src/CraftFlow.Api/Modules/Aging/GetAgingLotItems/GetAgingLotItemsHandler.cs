using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.GetAgingLotItems;
public class GetAgingLotItemsHandler : IRequestHandler<GetAgingLotItemsQuery, Result<List<GetAgingLotItemDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetAgingLotItemsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<GetAgingLotItemDto>>> Handle(GetAgingLotItemsQuery request, CancellationToken cancellationToken)
    {
        var lot = await _dbContext.AgingLots
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.AgingLotId, cancellationToken);

        if (lot is null)
        {
            return Result.Failure<List<GetAgingLotItemDto>>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        var dtos = lot.Items
            .Select(i => new GetAgingLotItemDto(
                i.Id,
                i.ItemNumber,
                i.InitialWeight,
                i.CurrentWeight,
                (int)i.State,
                i.DiscardReason
            ))
            .ToList();

        return Result.Success(dtos);
    }
}