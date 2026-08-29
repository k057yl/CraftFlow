namespace CraftFlow.Wpf.Models;
public sealed record MrpReportDto(DateTime GeneratedAt, List<MaterialRequirementDto> Requirements);