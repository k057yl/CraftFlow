using CraftFlow.Api.Common.Authorization;
using CraftFlow.Api.Common.Behaviors;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.Warehouses.CreateWarehouse;

public record CreateWarehouseCommand(
    string Name,
    WarehouseType Type,
    string? Address = null
) : IRequest<Result<Guid>>, IRequireQuotaValidation, IRequirePermission
{
    public QuotaType QuotaType => QuotaType.WarehousesCount;
    public AppPermission RequiredPermission => AppPermission.WAREHOUSES_MANAGE;
}