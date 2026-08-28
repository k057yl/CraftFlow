using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Sales.Domain
{
    public sealed class Customer : AggregateRoot, ITenantEntity
    {
        public Guid TenantId { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Phone { get; private set; }

        private Customer() { }

        public static Customer Create(string name, string? phone = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(ErrorCodes.Sales.CUSTOMER_NAME_REQUIRED);

            return new Customer
            {
                Id = Guid.NewGuid(),
                Name = name,
                Phone = phone
            };
        }
    }
}
