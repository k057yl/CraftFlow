using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.StockLots.AddStockLot;

public record AddStockLotCommand(
    Guid WarehouseId,
    Guid ItemId,
    Guid UnitOfMeasureId,
    decimal Quantity,
    decimal UnitPrice,
    string? BatchNumber,
    int? UnitsCount = null,
    Guid? SupplierId = null,
    DateTime? ExpirationDate = null,
    List<Guid>? StorageLocationIds = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PROCUREMENT_RECEIVE;
}