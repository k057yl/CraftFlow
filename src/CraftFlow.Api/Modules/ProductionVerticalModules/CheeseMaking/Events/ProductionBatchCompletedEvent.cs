using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Events;

public record ProductionBatchCompletedEvent(
    Guid BatchId,
    Guid RecipeId,
    Guid WarehouseId,
    decimal ActualOutputQuantity,
    int UnitsCount
) : INotification;