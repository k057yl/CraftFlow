using MediatR;
using static CraftFlow.SharedKernel.Constants.ErrorCodes;

namespace CraftFlow.Api.Modules.Production.Events;

public record ProductionBatchCompletedEvent(
    Guid BatchId,
    Guid RecipeId,
    Guid WarehouseId,
    decimal ActualOutputQuantity,
    int UnitsCount
) : INotification;