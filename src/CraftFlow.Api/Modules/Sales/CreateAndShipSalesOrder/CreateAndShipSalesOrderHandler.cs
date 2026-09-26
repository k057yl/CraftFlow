using CraftFlow.Api.Common.MultiTenancy;
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
    private readonly ITenantContext _tenantContext;

    public CreateAndShipSalesOrderHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateAndShipSalesOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.CustomerId == Guid.Empty || request.WarehouseId == Guid.Empty || request.Items.Count == 0)
        {
            return Result.Failure<Guid>(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));
        }

        var customerExists = await _dbContext.Customers
            .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (!customerExists)
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Sales.CUSTOMER_NAME_REQUIRED));

        var order = SalesOrder.Create(_tenantContext.TenantId, request.CustomerId, request.WarehouseId);

        var lotIds = request.Items.Select(i => i.StockLotId).ToList();

        var stockLots = await _dbContext.StockLots
            .Where(s => lotIds.Contains(s.Id) && s.IsActive)
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        foreach (var item in request.Items)
        {
            if (!stockLots.TryGetValue(item.StockLotId, out var lot))
            {
                return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Inventory.STOCK_LOT_NOT_FOUND));
            }

            if (lot.Quantity < item.Quantity)
            {
                return Result.Failure<Guid>(Error.Validation(ErrorCodes.Sales.INSUFFICIENT_PRODUCT_STOCK));
            }

            lot.AdjustQuantity(-item.Quantity);
            order.AddItem(lot.Id, item.Quantity, item.UnitPrice);
        }

        order.Ship();

        _dbContext.SalesOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}