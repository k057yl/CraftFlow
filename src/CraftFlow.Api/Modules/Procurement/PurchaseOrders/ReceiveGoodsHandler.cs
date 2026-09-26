using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Procurement.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Procurement.PurchaseOrders;

public sealed class ReceiveGoodsHandler : IRequestHandler<ReceiveGoodsCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public ReceiveGoodsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ReceiveGoodsCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Set<PurchaseOrder>()
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.Procurement.PURCHASE_ORDER_NOT_FOUND));
        }

        order.MarkAsReceived();

        foreach (var item in order.Items)
        {
            var batchNumber = string.Format(
                FormattingConstants.BATCH_NUMBER_FORMAT,
                DateTime.UtcNow,
                order.Id.ToString()[..4].ToUpperInvariant()
            );

            var stockLot = StockLot.Create(
                warehouseId: order.WarehouseId,
                itemId: item.RawMaterialId,
                initialQuantity: item.Quantity,
                unitsCount: 1,
                unitPrice: item.UnitPrice,
                batchNumber: batchNumber,
                tenantId: order.TenantId
            );

            await _dbContext.Set<StockLot>().AddAsync(stockLot, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}