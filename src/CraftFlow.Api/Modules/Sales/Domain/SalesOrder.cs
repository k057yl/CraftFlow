using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using Stateless;

namespace CraftFlow.Api.Modules.Sales.Domain;

public sealed class SalesOrder : AggregateRoot, ITenantEntity
{
    private StateMachine<OrderState, OrderTrigger>? _stateMachine;
    private readonly List<SalesOrderItem> _items = [];

    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public OrderState Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }

    public IReadOnlyCollection<SalesOrderItem> Items => _items.AsReadOnly();

    private SalesOrder() { }

    public static SalesOrder Create(Guid customerId, Guid warehouseId)
    {
        if (customerId == Guid.Empty || warehouseId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            WarehouseId = warehouseId,
            Status = OrderState.Draft,
            TotalAmount = 0m,
            CreatedAt = DateTime.UtcNow
        };

        order.InitStateMachine();
        return order;
    }

    public void AddItem(Guid stockLotId, decimal quantity, decimal unitPrice)
    {
        EnsureMachine();
        if (Status != OrderState.Draft)
            throw new InvalidOperationException(ErrorCodes.Sales.INVALID_ORDER_STATUS);

        var item = SalesOrderItem.Create(stockLotId, quantity, unitPrice);
        _items.Add(item);
        TotalAmount += quantity * unitPrice;
    }

    public void Confirm()
    {
        EnsureMachine();
        _stateMachine!.Fire(OrderTrigger.Confirm);
    }

    public void Ship()
    {
        EnsureMachine();
        ShippedAt = DateTime.UtcNow;
        _stateMachine!.Fire(OrderTrigger.Ship);
    }

    public void Cancel()
    {
        EnsureMachine();
        _stateMachine!.Fire(OrderTrigger.Cancel);
    }

    private void EnsureMachine()
    {
        if (_stateMachine is null)
            InitStateMachine();
    }

    private void InitStateMachine()
    {
        _stateMachine = new StateMachine<OrderState, OrderTrigger>(
            () => Status,
            s => Status = s
        );

        _stateMachine.Configure(OrderState.Draft)
            .Permit(OrderTrigger.Confirm, OrderState.Confirmed)
            .Permit(OrderTrigger.Ship, OrderState.Shipped)
            .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

        _stateMachine.Configure(OrderState.Confirmed)
            .Permit(OrderTrigger.Ship, OrderState.Shipped)
            .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

        _stateMachine.Configure(OrderState.Shipped);
        _stateMachine.Configure(OrderState.Cancelled);
    }
}