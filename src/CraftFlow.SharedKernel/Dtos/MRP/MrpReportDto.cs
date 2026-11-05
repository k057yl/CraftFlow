namespace CraftFlow.SharedKernel.Dtos.MRP;

public sealed record MrpReportDto(
    DateTime GeneratedAt,
    List<BatchMrpGroupDto> Batches,
    List<SummaryRequirementDto> SummaryRequirements
);