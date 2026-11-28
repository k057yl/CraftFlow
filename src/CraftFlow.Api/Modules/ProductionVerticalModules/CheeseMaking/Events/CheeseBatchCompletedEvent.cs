using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Events;

public record CheeseBatchCompletedEvent(
    Guid BatchId,
    Guid RecipeId,
    Guid WarehouseId,
    decimal ActualOutputQuantity,
    int UnitsCount,
    IReadOnlyList<decimal>? HeadWeights = null,
    DateTime? ExpirationDate = null
) : INotification;