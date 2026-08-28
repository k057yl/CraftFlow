namespace CraftFlow.Api.Common.MultiTenancy
{
    public interface ITenantContext
    {
        Guid TenantId { get; }
        Guid UserId { get; }
        bool IsResolved { get; }
    }
}
