namespace CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;

public record ProductDto(Guid Id, string Name, Guid UnitOfMeasureId, string UnitOfMeasureCode);