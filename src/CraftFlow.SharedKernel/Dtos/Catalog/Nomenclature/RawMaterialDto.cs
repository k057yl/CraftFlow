namespace CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;

public record RawMaterialDto(Guid Id, string Name, Guid UnitOfMeasureId, string UnitOfMeasureCode);