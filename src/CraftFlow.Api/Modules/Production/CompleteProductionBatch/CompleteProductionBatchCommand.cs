using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.CompleteProductionBatch;

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
) : IRequest<Result<Guid>>;