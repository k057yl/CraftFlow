using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Catalog.Domain;

public enum UnitType
{
    Weight = 1,
    Volume = 2,
    Piece = 3
}

public sealed class UnitOfMeasure : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public UnitType Type { get; private set; }

    public Guid? BaseUnitId { get; private set; }

    public decimal ConversionFactor { get; private set; } = 1.0m;

    private UnitOfMeasure() { }

    public static UnitOfMeasure Create(
        string name,
        string code,
        UnitType type,
        decimal conversionFactor = 1.0m,
        Guid? baseUnitId = null)
    {
        return new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Type = type,
            ConversionFactor = conversionFactor,
            BaseUnitId = baseUnitId
        };
    }
}