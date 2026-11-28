using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.CompleteCheeseBatch;

public sealed record CompleteCheeseBatchCommand(
    Guid BatchId,
    decimal ActualOutputQuantity,
    int UnitsCount,
    decimal? OverheadPercentage,
    string? BatchNumber,
    bool RequiresAging,
    Guid? AgingChamberId,
    int? MinAgingDays,
    Guid? StorageLocationId,
    List<decimal>? HeadWeights = null,
    DateTime? ExpirationDate = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}