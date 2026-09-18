using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;

namespace CraftFlow.Api.Common.MultiTenancy;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid UserId { get; }
    TenantRole Role { get; }
    bool IsSuperAdmin => Role == TenantRole.SuperAdmin || (TenantId == Guid.Empty && Role == TenantRole.Owner);
    bool IsResolved { get; }
}