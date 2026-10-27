using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.AgingProcess.DiscardAgingLot;

public record DiscardAgingLotCommand(
    Guid AgingLotId,
    decimal Quantity,
    int UnitsToRemove = 1,
    string? Reason = null
) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.INVENTORY_WRITEOFF;
}