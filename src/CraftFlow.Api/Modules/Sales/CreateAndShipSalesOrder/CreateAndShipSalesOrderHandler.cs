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
            var activeLots = await _dbContext.StockLots
                .Where(s => s.WarehouseId == request.WarehouseId && s.ItemId == item.ProductId && s.Quantity > 0)
                .OrderByDescending(s => s.Quantity)
                .ToListAsync(cancellationToken);

            var totalAvailableQuantity = activeLots.Sum(s => s.Quantity);

            if (activeLots.Count == 0 || totalAvailableQuantity < item.Quantity)
                return Result.Failure<Guid>(Error.Validation(ErrorCodes.Sales.INSUFFICIENT_PRODUCT_STOCK));

            decimal unitCost = activeLots.Sum(l => l.Quantity * l.UnitPrice) / totalAvailableQuantity;
            decimal basePrice = unitCost > 0 ? unitCost * 1.40m : 450.00m;
            decimal discountPercent = 0m;
            if (item.Quantity >= 20) discountPercent = 10m;
            else if (item.Quantity >= 10) discountPercent = 5m;

            var finalUnitPrice = Math.Round(basePrice * (1m - (discountPercent / 100m)), 2);
            var primaryLot = activeLots.First();
            primaryLot.AdjustQuantity(-item.Quantity);

            order.AddItem(primaryLot.Id, item.Quantity, finalUnitPrice);
        }

        order.Ship();

        _dbContext.SalesOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}