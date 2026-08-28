using System.Security.Claims;

namespace CraftFlow.Api.Common.Security;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var tenantClaim = user?.FindFirst("tenant_id")?.Value
                              ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();

            return Guid.TryParse(tenantClaim, out var tenantId)
                ? tenantId
                : Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}