using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.ReceiveStockBatch;

public sealed record ReceiveStockBatchCommand(
    Guid SupplierId,
    Guid WarehouseId,
    string? InvoiceNumber,
    List<StockReceiptLineDto> Items
) : IRequest<Result<List<Guid>>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PROCUREMENT_RECEIVE;
}