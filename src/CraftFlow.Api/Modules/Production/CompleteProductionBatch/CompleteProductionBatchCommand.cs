using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.CompleteProductionBatch;

public record CompleteProductionBatchCommand(
    Guid BatchId,
    decimal ActualOutputQuantity,
    int UnitsCount,
    string? BatchNumber = null
) : IRequest<Result<Guid>>;