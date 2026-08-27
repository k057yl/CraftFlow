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

        public static Warehouse Create(string name, string? address = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(ErrorCodes.Inventory.WAREHOUSE_NAME_REQUIRED);

            return new Warehouse
            {
                Id = Guid.NewGuid(),
                Name = name,
                Address = address
            };
        }
    }
}
