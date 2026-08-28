namespace CraftFlow.Api.Modules.Analytics.GetAuditLogs;
public record AuditLogDto(Guid Id, string EntityName, string Action, string Details, DateTime CreatedAtUtc);
