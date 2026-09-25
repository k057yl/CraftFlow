using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Sale;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sales.GetStockLotDetails;
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
            .FirstOrDefaultAsync(s => s.Id == request.StockLotId && s.IsActive, cancellationToken);

        if (stockLot == null)
        {
            return Result.Failure<StockLotDetailsDto>(Error.NotFound(ErrorCodes.Inventory.STOCK_LOT_NOT_FOUND));
        }

        var productName = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == stockLot.ItemId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? stockLot.BatchNumber;

        var items = new List<LotItemDetailsDto>();

        if (stockLot.ProductionBatchId.HasValue)
        {
            var agingLot = await _dbContext.AgingLots
                .AsNoTracking()
                .Include(a => a.Items)
                .FirstOrDefaultAsync(a => a.ProductionBatchId == stockLot.ProductionBatchId.Value && a.IsActive, cancellationToken);

            if (agingLot != null)
            {
                items = agingLot.Items
                    .Where(i => i.CurrentWeight > 0)
                    .Select(i => new LotItemDetailsDto(i.Id, i.ItemNumber, i.CurrentWeight))
                    .ToList();
            }
        }

        var result = new StockLotDetailsDto(
            stockLot.Id,
            stockLot.BatchNumber,
            productName,
            stockLot.Quantity,
            items.Count > 0 ? items.Count : stockLot.UnitsCount,
            items
        );

        return Result.Success(result);
    }
}