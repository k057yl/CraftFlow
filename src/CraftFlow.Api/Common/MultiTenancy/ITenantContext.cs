namespace CraftFlow.Api.Common.MultiTenancy
{
    public interface ITenantContext
    {
        Guid TenantId { get; }
        bool IsResolved { get; }
    }
}
