using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Production.Domain;

public sealed class ProductionBatch : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid RecipeId { get; private set; }
    public Guid TargetProductId { get; private set; }
    public Guid WarehouseId { get; private set; }

    public decimal PlannedOutputQuantity { get; private set; }
    public decimal ActualOutputQuantity { get; private set; }
    public BatchStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public string? DiscardReason { get; private set; }

    private ProductionBatch() { }

    public static ProductionBatch Create(
        Guid recipeId,
        Guid targetProductId,
        Guid warehouseId,
        Guid destinationWarehouseId,
        decimal plannedOutputQuantity,
        string? customName = null)
    {
        if (plannedOutputQuantity <= 0)
            throw new ArgumentException(ErrorCodes.Catalog.RECIPE_INVALID_TARGET_OUTPUT);

        var batchId = Guid.NewGuid();
        var defaultName = string.Concat(FormattingConstants.BATCH_PREFIX, batchId.ToString()[..8].ToUpperInvariant());

        return new ProductionBatch
        {
            Id = batchId,
            Name = string.IsNullOrWhiteSpace(customName) ? defaultName : customName.Trim(),
            RecipeId = recipeId,
            TargetProductId = targetProductId,
            WarehouseId = warehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            PlannedOutputQuantity = plannedOutputQuantity,
            ActualOutputQuantity = 0,
            Status = BatchStatus.Draft,
            StartedAt = DateTime.UtcNow
        };
    }

    public void Start()
    {
        if (Status != BatchStatus.Draft)
            throw new InvalidOperationException(ErrorCodes.Production.INVALID_STATUS_TRANSITION);

        Status = BatchStatus.InProgress;
    }

    public void Complete(decimal actualOutputQuantity)
    {
        if (Status != BatchStatus.InProgress && Status != BatchStatus.TransferredToAging)
            throw new InvalidOperationException(ErrorCodes.Production.INVALID_STATUS_TRANSITION);

        ActualOutputQuantity = actualOutputQuantity;
        Status = BatchStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Discard(string reason)
    {
        if (Status == BatchStatus.Cancelled)
            throw new InvalidOperationException(ErrorCodes.Production.INVALID_STATUS_TRANSITION);

        Status = BatchStatus.Cancelled;
        DiscardReason = reason;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsTransferredToAging()
    {
        if (Status != BatchStatus.Completed)
        {
            throw new InvalidOperationException(ErrorCodes.Production.ONLY_COMPLETED_BATCHES_CAN_BE_TRANSFERRED_TO_AGING);
        }

        Status = BatchStatus.TransferredToAging;
    }
}