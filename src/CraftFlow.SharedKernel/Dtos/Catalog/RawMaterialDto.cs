namespace CraftFlow.SharedKernel.Dtos.Catalog;

public record RawMaterialDto(Guid Id, string Name, Guid UnitOfMeasureId, string UnitOfMeasureCode);