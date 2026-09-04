using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.CompleteProductionBatch;

public record CompleteProductionBatchCommand(
    Guid BatchId,
    decimal ActualOutputQuantity,
    int UnitsCount,
    string? BatchNumber = null,
    decimal? OverheadPercentage = null
) : IRequest<Result<Guid>>;