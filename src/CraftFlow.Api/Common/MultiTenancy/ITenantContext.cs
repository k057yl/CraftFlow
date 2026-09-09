namespace CraftFlow.Api.Common.MultiTenancy;
public interface ITenantContext
{
    Guid TenantId { get; }
    Guid UserId { get; }
    bool IsAdmin { get; }
    bool IsSystemAdmin => TenantId == Guid.Empty && IsAdmin;
    bool IsResolved { get; }
}