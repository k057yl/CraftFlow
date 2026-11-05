namespace CraftFlow.SharedKernel.Dtos.MRP;

public sealed record BatchMrpGroupDto(
    Guid BatchId,
    string BatchName,
    string RecipeName,
    decimal PlannedOutputQuantity,
    DateTime? ScheduledAt,
    List<BatchMaterialRequirementDto> Ingredients
);