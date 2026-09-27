namespace CraftFlow.SharedKernel.Dtos.MRP;

public sealed record MrpReportDto(
    DateTime GeneratedAt,
    List<MaterialRequirementDto> Requirements
);