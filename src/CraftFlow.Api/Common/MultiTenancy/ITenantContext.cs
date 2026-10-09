using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.Api.Common.MultiTenancy;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid UserId { get; }
    string? UserEmail { get; }
    TenantRole Role { get; }
    bool IsSuperAdmin => Role == TenantRole.SuperAdmin || (TenantId == Guid.Empty && Role == TenantRole.Owner);
    bool IsResolved { get; }
}