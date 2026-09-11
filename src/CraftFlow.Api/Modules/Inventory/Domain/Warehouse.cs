using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Inventory.Domain
{
    public sealed class Warehouse : AggregateRoot, ITenantEntity
    {
        public Guid TenantId { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Address { get; private set; }

        private Warehouse() { }

        public static Warehouse Create(Guid tenantId, string name, string? address = null)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException(ErrorCodes.Auth.INVALID_CREDENTIALS);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(ErrorCodes.Inventory.WAREHOUSE_NAME_REQUIRED);

            return new Warehouse
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                Address = address
            };
        }
    }
}
