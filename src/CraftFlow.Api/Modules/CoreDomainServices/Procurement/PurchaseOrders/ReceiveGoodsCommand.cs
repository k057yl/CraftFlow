using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Procurement.PurchaseOrders;

public sealed record ReceiveGoodsCommand(Guid PurchaseOrderId) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PROCUREMENT_RECEIVE;
}