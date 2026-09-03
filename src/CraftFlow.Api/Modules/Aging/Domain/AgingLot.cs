using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using Stateless;

namespace CraftFlow.Api.Modules.Aging.Domain;

public sealed class AgingLot : AggregateRoot, ITenantEntity
{
    private StateMachine<AgingState, AgingTrigger>? _stateMachine;

    public Guid TenantId { get; private set; }
    public Guid ProductionBatchId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid AgingChamberId { get; private set; }
    public string BatchNumber { get; private set; } = null!;
    public int UnitsCount { get; private set; }

    public decimal InitialQuantity { get; private set; }
    public decimal CurrentQuantity { get; private set; }
    public AgingState State { get; private set; }

    public DateTime PlacedAt { get; private set; }
    public DateTime TargetReleaseDate { get; private set; }
    public DateTime? ActualReleaseDate { get; private set; }

    private AgingLot() { }

    public static AgingLot Create(
        Guid productionBatchId,
        Guid productId,
        Guid agingChamberId,
        string batchNumber,
        decimal initialQuantity,
        int unitsCount,
        int minAgingDays)
    {
        if (initialQuantity <= 0 || unitsCount <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        var now = DateTime.UtcNow;

        var lot = new AgingLot
        {
            Id = Guid.NewGuid(),
            ProductionBatchId = productionBatchId,
            ProductId = productId,
            AgingChamberId = agingChamberId,
            BatchNumber = batchNumber,
            UnitsCount = unitsCount,
            InitialQuantity = initialQuantity,
            CurrentQuantity = initialQuantity,
            State = AgingState.InChamber,
            PlacedAt = now,
            TargetReleaseDate = now.AddDays(minAgingDays)
        };

        lot.InitStateMachine();
        return lot;
    }

    public void RegisterLoss(decimal actualQuantity, int actualUnitsCount)
    {
        EnsureMachine();

        if (_stateMachine!.State != AgingState.InChamber && _stateMachine!.State != AgingState.ReadyForRelease)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        if (actualQuantity <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        if (actualUnitsCount <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        CurrentQuantity = actualQuantity;
        UnitsCount = actualUnitsCount;
    }

    public void Release()
    {
        EnsureMachine();

        ActualReleaseDate = DateTime.UtcNow;
        _stateMachine!.Fire(AgingTrigger.Release);
    }

    public void Discard()
    {
        EnsureMachine();

        ActualReleaseDate = DateTime.UtcNow;
        _stateMachine!.Fire(AgingTrigger.Discard);
    }

    private void EnsureMachine()
    {
        if (_stateMachine is null)
            InitStateMachine();
    }

    private void InitStateMachine()
    {
        _stateMachine = new StateMachine<AgingState, AgingTrigger>(
            () => State,
            s => State = s
        );

        _stateMachine.Configure(AgingState.InChamber)
            .Permit(AgingTrigger.MarkReadyForRelease, AgingState.ReadyForRelease)
            .Permit(AgingTrigger.Release, AgingState.Released)
            .Permit(AgingTrigger.Discard, AgingState.Discarded);

        _stateMachine.Configure(AgingState.ReadyForRelease)
            .Permit(AgingTrigger.Release, AgingState.Released)
            .Permit(AgingTrigger.Discard, AgingState.Discarded);
    }
}