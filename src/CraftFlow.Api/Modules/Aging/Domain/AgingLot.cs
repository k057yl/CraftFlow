using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using Stateless;

namespace CraftFlow.Api.Modules.Aging.Domain;

public sealed class AgingLot : AggregateRoot, ITenantEntity
{
    private StateMachine<AgingState, AgingTrigger>? _stateMachine;
    private readonly List<AgingLotItem> _items = [];

    public Guid TenantId { get; private set; }
    public Guid ProductionBatchId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid AgingChamberId { get; private set; }
    public Guid? StorageLocationId { get; private set; }
    public string BatchNumber { get; private set; } = null!;

    public IReadOnlyCollection<AgingLotItem> Items => _items.AsReadOnly();

    public int UnitsCount => _items.Count(i => i.State == AgingItemState.InChamber);
    public decimal InitialQuantity { get; private set; }
    public decimal CurrentQuantity => _items.Where(i => i.State == AgingItemState.InChamber).Sum(i => i.CurrentWeight);

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
        int minAgingDays,
        Guid? storageLocationId = null)
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
            StorageLocationId = storageLocationId,
            BatchNumber = batchNumber,
            InitialQuantity = initialQuantity,
            State = AgingState.InChamber,
            PlacedAt = now,
            TargetReleaseDate = now.AddDays(minAgingDays)
        };

        lot.PopulateItems(unitsCount, initialQuantity);
        lot.InitStateMachine();
        return lot;
    }

    public void PopulateItems(int unitsCount, decimal totalWeight)
    {
        _items.Clear();
        decimal avgWeight = Math.Round(totalWeight / unitsCount, 3);

        for (int i = 1; i <= unitsCount; i++)
        {
            var itemCode = $"{BatchNumber}-{i:D2}";
            _items.Add(AgingLotItem.Create(Id, itemCode, avgWeight));
        }
    }

    public void DiscardUnits(decimal quantity, int unitsToRemove, string reason)
    {
        EnsureMachine();

        if (_stateMachine!.State != AgingState.InChamber && _stateMachine!.State != AgingState.ReadyForRelease)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        var activeItems = _items.Where(i => i.State == AgingItemState.InChamber).ToList();

        if (activeItems.Count == 0)
            return;

        if (unitsToRemove > 0)
        {
            var itemsToDiscard = activeItems.Take(unitsToRemove).ToList();
            foreach (var item in itemsToDiscard)
            {
                item.Discard(reason);
            }

            activeItems = _items.Where(i => i.State == AgingItemState.InChamber).ToList();
        }

        if (quantity > 0 && unitsToRemove == 0 && activeItems.Count > 0)
        {
            decimal lossPerItem = Math.Round(quantity / activeItems.Count, 3);
            foreach (var item in activeItems)
            {
                item.RegisterWeightLoss(lossPerItem);
            }
        }

        if (_items.Count > 0 && _items.All(i => i.State == AgingItemState.Discarded))
        {
            ActualReleaseDate = DateTime.UtcNow;
            _stateMachine!.Fire(AgingTrigger.Discard);
        }
    }

    public void RegisterLoss(decimal actualQuantity, int actualUnitsCount)
    {
        EnsureMachine();

        if (_stateMachine!.State != AgingState.InChamber && _stateMachine!.State != AgingState.ReadyForRelease)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        if (actualQuantity <= 0 || actualUnitsCount <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        decimal diffWeight = CurrentQuantity - actualQuantity;
        int diffUnits = UnitsCount - actualUnitsCount;

        DiscardUnits(diffWeight > 0 ? diffWeight : 0, diffUnits > 0 ? diffUnits : 0, ErrorCodes.Aging.DISCARD_REASON_INVENTORY_LOSS);
    }

    public void Release()
    {
        EnsureMachine();

        foreach (var item in _items.Where(i => i.State == AgingItemState.InChamber))
        {
            item.Release();
        }

        ActualReleaseDate = DateTime.UtcNow;
        _stateMachine!.Fire(AgingTrigger.Release);
    }

    public void Discard()
    {
        EnsureMachine();

        foreach (var item in _items.Where(i => i.State == AgingItemState.InChamber))
        {
            item.Discard(ErrorCodes.Aging.DISCARD_REASON_MANUAL_DISCARD);
        }

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