using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Production.Batches.CompleteProductionBatch;

public sealed record CompleteProductionBatchCommand(
    Guid BatchId,
    decimal ActualOutputQuantity,
    int UnitsCount,
    decimal? OverheadPercentage,
    string? BatchNumber,
    bool RequiresAging,
    Guid? AgingChamberId,
    int? MinAgingDays,
    Guid? StorageLocationId
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}