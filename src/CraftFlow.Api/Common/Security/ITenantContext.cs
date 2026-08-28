namespace CraftFlow.Api.Common.Security;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid UserId { get; }
}
