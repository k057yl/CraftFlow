using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Events;

public class RecalculateStockCostHandler : INotificationHandler<CheeseBatchCompletedEvent>
{
    private readonly ILogger<RecalculateStockCostHandler> _logger;

    public RecalculateStockCostHandler(ILogger<RecalculateStockCostHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CheeseBatchCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "EVENT CONSUMED: Cheese Batch {BatchId} completed. Output: {OutputQty} units. Recalculating unit cost...",
            notification.BatchId,
            notification.ActualOutputQuantity
        );

        return Task.CompletedTask;
    }
}