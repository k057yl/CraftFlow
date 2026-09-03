using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Sales.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sales.CreateAndShipSalesOrder;

public class CreateAndShipSalesOrderHandler : IRequestHandler<CreateAndShipSalesOrderCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public CreateAndShipSalesOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateAndShipSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var customerExists = await _dbContext.Customers
            .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (!customerExists)
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Sales.CUSTOMER_NAME_REQUIRED));

        var order = SalesOrder.Create(request.CustomerId, request.WarehouseId);

        foreach (var item in request.Items)
        {
            var lot = await _dbContext.StockLots
                .FirstOrDefaultAsync(s => s.Id == item.StockLotId && s.WarehouseId == request.WarehouseId, cancellationToken);

            if (lot is null)
                return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Inventory.ITEM_NOT_FOUND));

            if (lot.Quantity < item.Quantity)
                return Result.Failure<Guid>(Error.Validation(ErrorCodes.Sales.INSUFFICIENT_PRODUCT_STOCK));

            lot.AdjustQuantity(-item.Quantity);

            order.AddItem(lot.Id, item.Quantity, item.UnitPrice);
        }

        order.Ship();

        _dbContext.SalesOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}