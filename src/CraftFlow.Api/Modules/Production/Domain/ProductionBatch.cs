using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Inventory;
using Stateless;

namespace CraftFlow.Api.Modules.Production.Domain;

public sealed class ProductionBatch : AggregateRoot, ITenantEntity
{
    private StateMachine<BatchState, BatchTrigger>? _stateMachine;

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid RecipeId { get; private set; }
    public Guid TargetProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public Guid EquipmentLocationId { get; private set; }

    public decimal PlannedOutputQuantity { get; private set; }
    public decimal ActualOutputQuantity { get; private set; }
    public decimal? OverheadPercentage { get; private set; }
    public BatchState State { get; private set; }

    public DateTime StartedAt { get; private set; }
    public DateTime? BrewingCompletedAt { get; private set; }
    public DateTime? AgingStartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public int TargetDurationMinutes { get; private set; }
    public bool IsTelegramNotified { get; private set; }
    public string? DiscardReason { get; private set; }
    public int UnitsCount { get; private set; } = 1;

    private ProductionBatch() { }

    public static ProductionBatch Create(
        Guid recipeId,
        Guid targetProductId,
        Guid warehouseId,
        Guid destinationWarehouseId,
        Guid equipmentLocationId,
        decimal plannedOutputQuantity,
        int targetDurationMinutes,
        string? customName = null,
        DateTime? scheduledAt = null)
    {
        if (plannedOutputQuantity <= 0)
            throw new ArgumentException(ErrorCodes.Catalog.RECIPE_INVALID_TARGET_OUTPUT);

        var batchId = Guid.NewGuid();
        var defaultName = string.Concat(FormattingConstants.BATCH_PREFIX, batchId.ToString()[..8].ToUpperInvariant());

        var batch = new ProductionBatch
        {
            Id = batchId,
            Name = string.IsNullOrWhiteSpace(customName) ? defaultName : customName.Trim(),
            RecipeId = recipeId,
            TargetProductId = targetProductId,
            WarehouseId = warehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            EquipmentLocationId = equipmentLocationId,
            PlannedOutputQuantity = plannedOutputQuantity,
            TargetDurationMinutes = targetDurationMinutes > 0 ? targetDurationMinutes : 180,
            ActualOutputQuantity = 0,
            State = BatchState.DRAFT,
            StartedAt = scheduledAt ?? DateTime.UtcNow,
            IsTelegramNotified = false
        };

        batch.InitStateMachine();
        return batch;
    }

    public void Start()
    {
        EnsureMachine();
        _stateMachine!.Fire(BatchTrigger.START);
    }

    public void Complete(decimal actualOutputQuantity, int unitsCount = 1, decimal? overheadPercentage = null)
    {
        EnsureMachine();
        ActualOutputQuantity = actualOutputQuantity;
        UnitsCount = unitsCount > 0 ? unitsCount : 1;
        SetOverheadPercentage(overheadPercentage);
        BrewingCompletedAt = DateTime.UtcNow;
        CompletedAt = DateTime.UtcNow;
        _stateMachine!.Fire(BatchTrigger.COMPLETE);
    }

    public void SetOverheadPercentage(decimal? percentage)
    {
        if (percentage.HasValue && percentage.Value < 0)
            throw new ArgumentException(ErrorCodes.General.VALIDATION_ERROR);

        OverheadPercentage = percentage;
    }

    public decimal CalculateTotalCost(decimal rawMaterialCost)
    {
        if (!OverheadPercentage.HasValue || OverheadPercentage.Value <= 0)
        {
            return rawMaterialCost;
        }

        var multiplier = 1m + (OverheadPercentage.Value / 100m);
        return rawMaterialCost * multiplier;
    }

    public void MarkReadyForAging(decimal actualOutputQuantity, int unitsCount = 1)
    {
        EnsureMachine();
        ActualOutputQuantity = actualOutputQuantity;
        UnitsCount = unitsCount > 0 ? unitsCount : 1;
        BrewingCompletedAt = DateTime.UtcNow;
        _stateMachine!.Fire(BatchTrigger.COMPLETE);
    }

    public void MarkAsTransferredToAging()
    {
        EnsureMachine();
        AgingStartedAt = DateTime.UtcNow;
        _stateMachine!.Fire(BatchTrigger.TRANSFER_TO_AGING);
    }

    public void ReleaseFromAging(decimal finalQuantity)
    {
        EnsureMachine();
        ActualOutputQuantity = finalQuantity;
        CompletedAt = DateTime.UtcNow;
        _stateMachine!.Fire(BatchTrigger.RELEASE_FROM_AGING);
    }

    public void Discard(string reason)
    {
        EnsureMachine();
        DiscardReason = reason;
        CompletedAt = DateTime.UtcNow;
        _stateMachine!.Fire(BatchTrigger.DISCARD);
    }

    public void MarkTelegramNotified()
    {
        IsTelegramNotified = true;
    }

    private void EnsureMachine()
    {
        if (_stateMachine is null)
            InitStateMachine();
    }

    public void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            Name = newName.Trim();
        }
    }

    private void InitStateMachine()
    {
        _stateMachine = new StateMachine<BatchState, BatchTrigger>(
            () => State,
            s => State = s
        );

        _stateMachine.Configure(BatchState.DRAFT)
            .Permit(BatchTrigger.START, BatchState.IN_PROGRESS);

        _stateMachine.Configure(BatchState.IN_PROGRESS)
            .Permit(BatchTrigger.COMPLETE, BatchState.COMPLETED)
            .Permit(BatchTrigger.TRANSFER_TO_AGING, BatchState.IN_AGING)
            .Permit(BatchTrigger.DISCARD, BatchState.DISCARDED);

        _stateMachine.Configure(BatchState.COMPLETED)
            .Permit(BatchTrigger.TRANSFER_TO_AGING, BatchState.IN_AGING);

        _stateMachine.Configure(BatchState.READY_FOR_AGING)
            .Permit(BatchTrigger.TRANSFER_TO_AGING, BatchState.IN_AGING)
            .Permit(BatchTrigger.DISCARD, BatchState.DISCARDED);

        _stateMachine.Configure(BatchState.IN_AGING)
            .Permit(BatchTrigger.RELEASE_FROM_AGING, BatchState.COMPLETED)
            .Permit(BatchTrigger.DISCARD, BatchState.DISCARDED);
    }
}