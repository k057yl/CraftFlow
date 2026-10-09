using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Common.Audit;

public sealed class AuditLog : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string UserEmail { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string Details { get; private set; } = null!;
    public string ChangesJson { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
    Guid tenantId,
    Guid userId,
    string userEmail,
    string entityName,
    string action,
    string details,
    string changesJson)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            UserEmail = userEmail,
            EntityName = entityName,
            Action = action,
            Details = details,
            ChangesJson = changesJson,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
    }
}