using CraftFlow.Api.Common.Behaviors;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.Warehouses.CreateWarehouse;

public record CreateWarehouseCommand(
    string Name,
    WarehouseType Type,
    string? Address = null
) : IRequest<Result<Guid>>, IRequireQuotaValidation
{
    public QuotaType QuotaType => QuotaType.WarehousesCount;
}