namespace CraftFlow.SharedKernel.Dtos.Production;

public record RequirementItemDto(
    string MaterialName,
    decimal RequiredQty,
    decimal AvailableQty,
    bool IsSufficient
);