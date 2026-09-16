using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sales.GetSalesStockInfo;

public class GetSalesStockInfoHandler : IRequestHandler<GetSalesStockInfoQuery, Result<SalesStockInfoDto>>
{
    private readonly AppDbContext _dbContext;

    public GetSalesStockInfoHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SalesStockInfoDto>> Handle(GetSalesStockInfoQuery request, CancellationToken cancellationToken)
    {
        var activeLots = await _dbContext.StockLots
            .AsNoTracking()
            .Where(s => s.WarehouseId == request.WarehouseId && s.ItemId == request.ProductId && s.Quantity > 0)
            .ToListAsync(cancellationToken);

        var totalQuantity = activeLots.Sum(s => s.Quantity);

        decimal unitCost = 0m;
        if (totalQuantity > 0)
        {
            unitCost = activeLots.Sum(l => l.Quantity * l.UnitPrice) / totalQuantity;
        }

        decimal basePrice = unitCost > 0 ? unitCost * 1.40m : 450.00m;

        return Result.Success(new SalesStockInfoDto(totalQuantity, unitCost, basePrice));
    }
}