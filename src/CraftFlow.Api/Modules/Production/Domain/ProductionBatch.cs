using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
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

    public decimal PlannedOutputQuantity { get; private set; }
    public decimal ActualOutputQuantity { get; private set; }
    public BatchState Status { get; private set; }
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

        var batch = new ProductionBatch
        {
            Id = batchId,
            Name = string.IsNullOrWhiteSpace(customName) ? defaultName : customName.Trim(),
            RecipeId = recipeId,
            TargetProductId = targetProductId,
            WarehouseId = warehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            PlannedOutputQuantity = plannedOutputQuantity,
            ActualOutputQuantity = 0,
            Status = BatchState.Draft,
            StartedAt = DateTime.UtcNow
        };

        batch.InitStateMachine();
        return batch;
    }

    public void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            Name = newName.Trim();
        }
    }

    public void Start()
    {
        EnsureMachine();
        _stateMachine!.Fire(BatchTrigger.Start);
    }

    public void Complete(decimal actualOutputQuantity)
    {
        EnsureMachine();
        ActualOutputQuantity = actualOutputQuantity;
        CompletedAt = DateTime.UtcNow;
        _stateMachine!.Fire(BatchTrigger.Complete);
    }

    public void MarkAsTransferredToAging()
    {
        EnsureMachine();
        _stateMachine!.Fire(BatchTrigger.TransferToAging);
    }

    public void ReleaseFromAging(decimal finalQuantity)
    {
        EnsureMachine();
        ActualOutputQuantity = finalQuantity;
        CompletedAt = DateTime.UtcNow;
        _stateMachine!.Fire(BatchTrigger.ReleaseFromAging);
    }

    public void Discard(string reason)
    {
        EnsureMachine();
        DiscardReason = reason;
        CompletedAt = DateTime.UtcNow;
        _stateMachine!.Fire(BatchTrigger.Discard);
    }

    private void EnsureMachine()
    {
        if (_stateMachine is null)
            InitStateMachine();
    }

    private void InitStateMachine()
    {
        _stateMachine = new StateMachine<BatchState, BatchTrigger>(
            () => Status,
            s => Status = s
        );

        _stateMachine.Configure(BatchState.Draft)
            .Permit(BatchTrigger.Start, BatchState.InProgress);

        _stateMachine.Configure(BatchState.InProgress)
            .Permit(BatchTrigger.Complete, BatchState.Completed)
            .Permit(BatchTrigger.TransferToAging, BatchState.InAging)
            .Permit(BatchTrigger.Discard, BatchState.Discarded);

        _stateMachine.Configure(BatchState.ReadyForAging)
            .Permit(BatchTrigger.TransferToAging, BatchState.InAging)
            .Permit(BatchTrigger.Discard, BatchState.Discarded);

        _stateMachine.Configure(BatchState.Completed)
            .Permit(BatchTrigger.TransferToAging, BatchState.InAging);

        _stateMachine.Configure(BatchState.InAging)
            .Permit(BatchTrigger.ReleaseFromAging, BatchState.Completed)
            .Permit(BatchTrigger.Discard, BatchState.Discarded);
    }
}