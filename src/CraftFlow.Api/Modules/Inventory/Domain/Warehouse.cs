using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Inventory;

namespace CraftFlow.Api.Modules.Inventory.Domain;

public sealed class Warehouse : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Address { get; private set; }
    public WarehouseType Type { get; private set; }

    private Warehouse() { }

    public static Warehouse Create(
        Guid tenantId,
        string name,
        WarehouseType type = WarehouseType.WAREHOUSE,
        string? address = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Tenant.INVALID_TENANT_ID);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorCodes.Inventory.WAREHOUSE_NAME_REQUIRED);

        return new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Type = type,
            Address = address
        };
    }
}