using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.MRP.CreateProcurement;

public sealed record CreateProcurementFromMrpCommand(
    Guid SupplierId,
    Guid WarehouseId,
    List<MrpPurchaseItemRequestDto> Items
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PROCUREMENT_RECEIVE;
}