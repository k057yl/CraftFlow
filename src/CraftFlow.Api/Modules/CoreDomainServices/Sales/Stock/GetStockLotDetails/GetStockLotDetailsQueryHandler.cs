using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Sale;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sales.Stock.GetStockLotDetails;

public class GetStockLotDetailsQueryHandler : IRequestHandler<GetStockLotDetailsQuery, Result<StockLotDetailsDto>>
{
    private readonly AppDbContext _dbContext;

    public GetStockLotDetailsQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<StockLotDetailsDto>> Handle(GetStockLotDetailsQuery request, CancellationToken cancellationToken)
    {
        var stockLot = await _dbContext.StockLots
            .AsNoTracking()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == request.StockLotId && s.IsActive, cancellationToken);

        if (stockLot == null)
        {
            return Result.Failure<StockLotDetailsDto>(Error.NotFound(ErrorCodes.Inventory.STOCK_LOT_NOT_FOUND));
        }

        var productName = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == stockLot.ItemId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? stockLot.BatchNumber ?? string.Empty;

        var items = stockLot.Items
            .Where(i => !i.IsSold && i.CurrentWeight > 0)
            .OrderBy(i => i.ItemNumber)
            .Select(i => new LotItemDetailsDto(i.Id, i.ItemNumber, i.CurrentWeight))
            .ToList();

        var result = new StockLotDetailsDto(
            stockLot.Id,
            stockLot.BatchNumber ?? string.Empty,
            productName,
            stockLot.Quantity,
            items.Count > 0 ? items.Count : stockLot.UnitsCount,
            items
        );

        return Result.Success(result);
    }
}