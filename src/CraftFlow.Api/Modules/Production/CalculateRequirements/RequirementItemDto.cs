namespace CraftFlow.Api.Modules.Production.CalculateRequirements;

public record RequirementItemDto(
    string MaterialName,
    decimal RequiredQty,
    decimal AvailableQty,
    bool IsSufficient
);