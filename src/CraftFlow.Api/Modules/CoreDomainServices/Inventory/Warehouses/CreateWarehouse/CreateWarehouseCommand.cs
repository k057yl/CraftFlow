using CraftFlow.Api.Core.Authorization;
using CraftFlow.Api.Core.Behaviors;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.Warehouses.CreateWarehouse;

public record CreateWarehouseCommand(
    string Name,
    WarehouseType Type,
    string? Address = null
) : IRequest<Result<Guid>>, IRequireQuotaValidation, IRequirePermission
{
    public QuotaType QuotaType => QuotaType.WAREHOUSES_COUNT;
    public AppPermission RequiredPermission => AppPermission.WAREHOUSES_MANAGE;
}