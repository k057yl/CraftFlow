using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.AgingProcess.DiscardSpecificAgingItems;

public sealed record DiscardSpecificAgingItemsCommand(
    Guid AgingLotId,
    List<Guid> ItemIds,
    string Reason
) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.INVENTORY_WRITEOFF;
}