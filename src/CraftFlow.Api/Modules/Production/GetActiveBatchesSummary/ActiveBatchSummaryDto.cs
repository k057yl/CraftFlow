namespace CraftFlow.Api.Modules.Production.GetActiveBatchesSummary;

public sealed record ActiveBatchSummaryDto(
    Guid Id,
    string BatchName,
    string RecipeName,
    decimal PlannedOutputQuantity,
    DateTime StartedAt,
    int TargetDurationMinutes,
    int ElapsedMinutes,
    bool IsOverdue,
    string State
);