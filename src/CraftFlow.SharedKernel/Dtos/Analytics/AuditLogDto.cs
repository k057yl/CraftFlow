namespace CraftFlow.SharedKernel.Dtos.Analytics;

public record AuditLogDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string Action,
    string Details,
    DateTime CreatedAtUtc
);