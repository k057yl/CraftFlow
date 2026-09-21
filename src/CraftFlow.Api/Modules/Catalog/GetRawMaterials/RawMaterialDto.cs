namespace CraftFlow.Api.Modules.Catalog.GetRawMaterials;

public record RawMaterialDto(Guid Id, string Name, Guid UnitOfMeasureId, string UnitOfMeasureCode);