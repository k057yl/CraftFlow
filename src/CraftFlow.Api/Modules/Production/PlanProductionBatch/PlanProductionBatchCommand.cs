using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.PlanProductionBatch;

public sealed record PlanProductionBatchCommand(
    Guid RecipeId,
    Guid WarehouseId,
    Guid DestinationWarehouseId,
    decimal PlannedOutputQuantity,
    string? Name = null,
    DateTime? ScheduledAt = null
) : IRequest<Result<Guid>>;