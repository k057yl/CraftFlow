using MediatR;

namespace CraftFlow.Api.Modules.Production.Events;

public record ProductionBatchCompletedEvent(
    Guid BatchId,
    Guid RecipeId,
    Guid WarehouseId,
    decimal ActualOutputQuantity
) : INotification;