namespace CraftFlow.SharedKernel.Dtos.Sanitation;

public sealed record SanitationLogDto(
    Guid Id,
    Guid TaskId,
    Guid PerformedByUserId,
    Guid VerifiedByUserId,
    DateTime StartedAt,
    DateTime CompletedAt,
    DateTime VerifiedAt,
    int DurationMinutes,
    string ChemicalsUsed,
    string? Notes,
    string? VerificationNotes,
    bool IsPassed
);