using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Catalog.Domain
{
    public sealed class UnitOfMeasure : Entity, ITenantEntity
    {
        public Guid TenantId { get; private set; }
        public string Name { get; private set; } = null!;
        public string Code { get; private set; } = null!;

        private UnitOfMeasure() { }

        public static UnitOfMeasure Create(string name, string code)
        {
            return new UnitOfMeasure
            {
                Id = Guid.NewGuid(),
                Name = name,
                Code = code
            };
        }
    }
}
