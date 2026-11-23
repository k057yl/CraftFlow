using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Sales.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sales.Orders.CreateAndShipSalesOrder;

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
            .Include(s => s.StorageLocations)
            .Include(s => s.Items)
            .Where(s => lotIds.Contains(s.Id) && s.IsActive)
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var allStorageLocationIds = stockLots.Values
            .SelectMany(l => l.StorageLocations)
            .Select(sl => sl.StorageLocationId)
            .Distinct()
            .ToList();

        var storageLocations = await _dbContext.StorageLocations
            .Where(sl => allStorageLocationIds.Contains(sl.Id))
            .ToDictionaryAsync(sl => sl.Id, cancellationToken);

        foreach (var item in request.Items)
        {
            if (!stockLots.TryGetValue(item.StockLotId, out var lot))
            {
                return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Inventory.STOCK_LOT_NOT_FOUND));
            }

            var unsoldItems = lot.Items.Where(i => !i.IsSold).ToList();
            decimal shippedQuantity;

            if (unsoldItems.Count > 0)
            {
                var targetItemIds = (item.StockLotItemIds != null && item.StockLotItemIds.Count > 0)
                    ? item.StockLotItemIds
                    : unsoldItems.Select(i => i.Id).ToList();

                try
                {
                    shippedQuantity = lot.ShipSpecificItems(targetItemIds);
                }
                catch (InvalidOperationException)
                {
                    return Result.Failure<Guid>(Error.Validation(ErrorCodes.Sales.INSUFFICIENT_PRODUCT_STOCK));
                }
            }
            else
            {
                if (lot.Quantity < item.Quantity)
                {
                    return Result.Failure<Guid>(Error.Validation(ErrorCodes.Sales.INSUFFICIENT_PRODUCT_STOCK));
                }

                shippedQuantity = item.Quantity;
                lot.AdjustQuantity(-shippedQuantity);
            }

            order.AddItem(lot.Id, shippedQuantity, item.UnitPrice);

            if (lot.StorageLocations.Count > 0)
            {
                decimal remainingToFree = shippedQuantity;

                foreach (var stockLocation in lot.StorageLocations.ToList())
                {
                    if (remainingToFree <= 0) break;

                    if (storageLocations.TryGetValue(stockLocation.StorageLocationId, out var location))
                    {
                        decimal amountToFree = Math.Min(stockLocation.AllocatedQuantity, remainingToFree);

                        location.AddVolume(-amountToFree);
                        stockLocation.UpdateQuantity(stockLocation.AllocatedQuantity - amountToFree);

                        if (location.CurrentVolume <= 0.001m)
                        {
                            location.ClearVolume();
                        }

                        remainingToFree -= amountToFree;
                    }
                }
            }
        }

        order.Ship();

        _dbContext.SalesOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}