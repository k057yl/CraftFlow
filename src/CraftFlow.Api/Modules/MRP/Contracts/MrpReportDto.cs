namespace CraftFlow.Api.Modules.MRP.Contracts;
public sealed record MrpReportDto(
    DateTime GeneratedAt,
    List<MaterialRequirementDto> Requirements
);