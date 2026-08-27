using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Catalog.Domain
{
    public sealed class Product : Entity, ITenantEntity
    {
        public Guid TenantId { get; private set; }
        public string Name { get; private set; } = null!;
        public Guid UnitOfMeasureId { get; private set; }

        private Product() { }

        public static Product Create(string name, Guid unitOfMeasureId)
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                UnitOfMeasureId = unitOfMeasureId
            };
        }

        public void Rename(string name) => Name = name;
    }
}
