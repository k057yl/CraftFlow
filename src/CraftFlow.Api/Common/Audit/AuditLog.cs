using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Common.Audit;

public sealed class AuditLog : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string EntityName { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string Details { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(Guid tenantId, Guid userId, string entityName, string action, string details)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            EntityName = entityName,
            Action = action,
            Details = details,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}