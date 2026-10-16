using CraftFlow.Api.Modules.Catalog.Domain;
using CraftFlow.SharedKernel.Constants;

namespace CraftFlow.Api.Modules.Catalog.Converter;

public static class UnitConverter
{
    public static decimal Convert(decimal quantity, UnitOfMeasure source, UnitOfMeasure target)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), ErrorCodes.Catalog.UOM_SOURCE_NULL);
        }

        if (target == null)
        {
            throw new ArgumentNullException(nameof(target), ErrorCodes.Catalog.UOM_TARGET_NULL);
        }

        if (source.Type != target.Type)
        {
            throw new InvalidOperationException(ErrorCodes.Catalog.UOM_TYPE_MISMATCH);
        }

        decimal baseQuantity = quantity * source.ConversionFactor;

        return baseQuantity / target.ConversionFactor;
    }

    public static decimal ToBaseUnit(decimal quantity, UnitOfMeasure source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), ErrorCodes.Catalog.UOM_SOURCE_NULL);
        }

        return quantity * source.ConversionFactor;
    }
}