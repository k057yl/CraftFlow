using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Catalog.Domain
{
    public sealed class RawMaterial : Entity, ITenantEntity
    {
        public Guid TenantId { get; private set; }
        public string Name { get; private set; } = null!;
        public Guid UnitOfMeasureId { get; private set; }

        private RawMaterial() { }

        public static RawMaterial Create(string name, Guid unitOfMeasureId)
        {
            return new RawMaterial
            {
                Id = Guid.NewGuid(),
                Name = name,
                UnitOfMeasureId = unitOfMeasureId
            };
        }

        public void Rename(string name) => Name = name;
    }
}
