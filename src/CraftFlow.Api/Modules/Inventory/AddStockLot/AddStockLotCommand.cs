using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.AddStockLot;

public record AddStockLotCommand(
    Guid WarehouseId,
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice,
    string? BatchNumber,
    int? UnitsCount = null,
    Guid? SupplierId = null,
    DateTime? ExpirationDate = null,
    List<Guid>? StorageLocationIds = null
) : IRequest<Result<Guid>>;