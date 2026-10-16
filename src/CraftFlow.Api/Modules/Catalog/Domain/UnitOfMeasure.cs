using CraftFlow.SharedKernel.Constants;
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
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorCodes.Catalog.UOM_INVALID_NAME);

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(ErrorCodes.Catalog.UOM_INVALID_CODE);

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type), ErrorCodes.Catalog.UOM_INVALID_TYPE);

        if (conversionFactor <= 0m)
            throw new ArgumentOutOfRangeException(nameof(conversionFactor), ErrorCodes.Catalog.UOM_INVALID_CONVERSION_FACTOR);

        if (baseUnitId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Catalog.UOM_INVALID_BASE_UNIT_ID, nameof(baseUnitId));

        if (baseUnitId is null && conversionFactor != 1.0m)
            throw new ArgumentException(ErrorCodes.Catalog.UOM_BASE_UNIT_FACTOR_MUST_BE_ONE);

        return new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code.Trim().ToLowerInvariant(),
            Type = type,
            ConversionFactor = conversionFactor,
            BaseUnitId = baseUnitId
        };
    }
}