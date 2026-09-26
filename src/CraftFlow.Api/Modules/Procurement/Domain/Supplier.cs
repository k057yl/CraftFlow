using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Procurement.Domain;

public sealed class Supplier : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    private Supplier() { }

    public static Supplier Create(Guid tenantId, string name, string? phone = null, string? email = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorCodes.Procurement.SUPPLIER_NAME_REQUIRED);

        return new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Phone = phone,
            Email = email
        };
    }

    public void UpdateInfo(string name, string? phone, string? email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorCodes.Procurement.SUPPLIER_NAME_REQUIRED);

        Name = name;
        Phone = phone;
        Email = email;
    }
}