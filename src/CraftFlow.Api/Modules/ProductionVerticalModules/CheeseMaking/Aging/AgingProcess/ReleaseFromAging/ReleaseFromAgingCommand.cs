using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.ReleaseFromAging;

public sealed record ReleaseFromAgingCommand(
    Guid AgingLotId,
    Guid TargetWarehouseId,
    decimal ActualFinalQuantity,
    int UnitsCount,
    decimal UnitPrice,
    string? CustomBatchNumber = null,
    List<Guid>? StorageLocationIds = null,
    List<decimal>? HeadWeights = null,
    List<Guid>? DiscardedItemIds = null
) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}