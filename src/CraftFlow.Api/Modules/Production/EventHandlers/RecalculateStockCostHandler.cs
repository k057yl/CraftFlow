using CraftFlow.Api.Modules.Production.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CraftFlow.Api.Modules.Production.EventHandlers;

public class RecalculateStockCostHandler : INotificationHandler<ProductionBatchCompletedEvent>
{
    private readonly ILogger<RecalculateStockCostHandler> _logger;

    public RecalculateStockCostHandler(ILogger<RecalculateStockCostHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProductionBatchCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "EVENT CONSUMED: Production Batch {BatchId} completed. Output: {OutputQty} units. Recalculating unit cost...",
            notification.BatchId,
            notification.ActualOutputQuantity
        );

        return Task.CompletedTask;
    }
}