using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Production.Events;

public record ProductionBatchCompletedEvent(
    Guid BatchId,
    Guid RecipeId,
    Guid WarehouseId,
    decimal ActualOutputQuantity,
    int UnitsCount
) : INotification;