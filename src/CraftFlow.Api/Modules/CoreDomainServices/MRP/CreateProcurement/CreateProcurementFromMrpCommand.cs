using CraftFlow.Api.Core.Authorization;
using CraftFlow.Api.Core.Behaviors;
using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.MRP.CreateProcurement;

public sealed record CreateProcurementFromMrpCommand(
    Guid SupplierId,
    Guid WarehouseId,
    List<MrpPurchaseItemRequestDto> Items
) : IRequest<Result<Guid>>, IRequirePermission, IRequirePlanFeature
{
    public AppPermission RequiredPermission => AppPermission.PROCUREMENT_RECEIVE;
    public PlanFeatures RequiredFeature => PlanFeatures.MRP_PLANNING;
}