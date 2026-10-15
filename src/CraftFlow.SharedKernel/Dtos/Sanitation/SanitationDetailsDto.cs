using CraftFlow.SharedKernel.Enums.Sanitation;

namespace CraftFlow.SharedKernel.Dtos.Sanitation;

public sealed record SanitationDetailsDto(
    Guid TargetId,
    SanitationTargetType TargetType,
    string TargetName,
    SanitationTriggerType? TriggerType,
    int? IntervalDays,
    int? MaxBatchCount,
    string? RequiredChemicals,
    DateTime? LastWashedAt,
    string LastWashedByUserName,
    DateTime? NextWashDueDate,
    string StatusText,
    Guid? ActiveTaskId,
    bool IsUnderSanitation,
    List<SanitationLogItemDto> Logs
);