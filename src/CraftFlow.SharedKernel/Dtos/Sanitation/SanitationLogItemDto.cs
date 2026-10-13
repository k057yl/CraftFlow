namespace CraftFlow.SharedKernel.Dtos.Sanitation;

public sealed record SanitationLogItemDto(
    Guid Id,
    string PerformedByUserName,
    string VerifiedByUserName,
    DateTime CompletedAt,
    DateTime VerifiedAt,
    int DurationMinutes,
    string ChemicalsUsed,
    string? Notes,
    bool IsPassed
);