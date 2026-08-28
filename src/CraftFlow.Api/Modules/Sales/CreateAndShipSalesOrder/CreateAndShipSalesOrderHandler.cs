using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Sales.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sales.CreateAndShipSalesOrder
{
    public class CreateAndShipSalesOrderHandler : IRequestHandler<CreateAndShipSalesOrderCommand, Result<Guid>>
    {
        private readonly AppDbContext _dbContext;

        public CreateAndShipSalesOrderHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(CreateAndShipSalesOrderCommand request, CancellationToken cancellationToken)
        {
            var order = SalesOrder.Create(request.CustomerId, request.WarehouseId);

            foreach (var item in request.Items)
            {
                var stockLots = await _dbContext.StockLots
                    .Where(s => s.WarehouseId == request.WarehouseId && s.ItemId == item.ProductId && s.Quantity > 0)
                    .ToListAsync(cancellationToken);

                var totalAvailable = stockLots.Sum(s => s.Quantity);

                if (totalAvailable < item.Quantity)
                {
                    return Result.Failure<Guid>(Error.Validation(ErrorCodes.Sales.INSUFFICIENT_PRODUCT_STOCK));
                }

                var remainingToDeduct = item.Quantity;
                foreach (var lot in stockLots)
                {
                    if (remainingToDeduct <= 0) break;

                    var deduct = Math.Min(lot.Quantity, remainingToDeduct);
                    lot.AdjustQuantity(-deduct);
                    remainingToDeduct -= deduct;
                }

                order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
            }

            order.Ship();

            _dbContext.SalesOrders.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(order.Id);
        }
    }
}
