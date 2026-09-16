using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.GetRawStockLots;
public class GetRawStockLotsHandler : IRequestHandler<GetRawStockLotsQuery, Result<List<RawStockLotLookupDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetRawStockLotsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<RawStockLotLookupDto>>> Handle(GetRawStockLotsQuery request, CancellationToken cancellationToken)
    {
        var rawLots = await _dbContext.StockLots
            .AsNoTracking()
            .Join(_dbContext.RawMaterials,
                sl => sl.ItemId,
                rm => rm.Id,
                (sl, rm) => new RawStockLotLookupDto(
                    sl.Id,
                    rm.Name + " (" + (sl.BatchNumber ?? "NoN") + " | Remainder: " + sl.Quantity + ")"
                ))
            .ToListAsync(cancellationToken);

        return Result.Success(rawLots);
    }
}