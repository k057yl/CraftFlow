namespace CraftFlow.SharedKernel.Dtos.Catalog;

public record ProductDto(Guid Id, string Name, Guid UnitOfMeasureId, string UnitOfMeasureCode);