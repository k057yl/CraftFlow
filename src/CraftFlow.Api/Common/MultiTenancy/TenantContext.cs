using CraftFlow.Api.Common.Constants;
using System.Security.Claims;

namespace CraftFlow.Api.Common.MultiTenancy;

public class TenantContext : ITenantContext
{
    private static readonly Guid DefaultTenantGuid = Guid.Parse(CoreConstants.MultiTenancy.DEFAULT_TENANT_ID_STRING);
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId => FetchTenantId();
    public Guid UserId => FetchUserId();
    public bool IsResolved => TryFetchTenantId(out _);

    private Guid FetchTenantId()
    {
        if (TryFetchTenantId(out var tenantId))
        {
            return tenantId;
        }

        return DefaultTenantGuid;
    }

    private Guid FetchUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return Guid.Empty;
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private bool TryFetchTenantId(out Guid tenantId)
    {
        tenantId = Guid.Empty;
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return false;
        }

        if (httpContext.Request.Headers.TryGetValue(CoreConstants.MultiTenancy.HEADER_TENANT_ID, out var headerValue) &&
            Guid.TryParse(headerValue, out tenantId))
        {
            return true;
        }

        var tenantClaim = httpContext.User.FindFirst(CoreConstants.MultiTenancy.CLAIM_TENANT_ID)?.Value;
        if (Guid.TryParse(tenantClaim, out tenantId))
        {
            return true;
        }

        return false;
    }
}