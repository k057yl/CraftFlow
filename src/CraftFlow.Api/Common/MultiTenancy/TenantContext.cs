using CraftFlow.Api.Common.Constants;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Identity;
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
    public string? UserEmail => FetchUserEmail();
    public TenantRole Role => FetchRole();
    public bool IsResolved => TryFetchTenantId(out _);

    private Guid FetchTenantId()
    {
        if (TryFetchTenantId(out var tenantId))
        {
            return tenantId;
        }

        return Guid.Empty;
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

    private string? FetchUserEmail()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User is null || !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        return httpContext.User.FindFirst(ClaimTypes.Email)?.Value
            ?? httpContext.User.FindFirst("email")?.Value
            ?? httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
    }

    private TenantRole FetchRole()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User is null || !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return TenantRole.None;
        }

        if (httpContext.User.IsInRole(AuthConstants.Roles.ADMIN) ||
            httpContext.User.HasClaim(c => (c.Type == ClaimTypes.Role || c.Type == AuthConstants.Claims.ROLE_SHORT) &&
                                           c.Value.Equals(AuthConstants.Roles.ADMIN, StringComparison.OrdinalIgnoreCase)))
        {
            return TenantRole.SuperAdmin;
        }

        var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role)?.Value
                        ?? httpContext.User.FindFirst(AuthConstants.Claims.ROLE_SHORT)?.Value;

        if (!string.IsNullOrWhiteSpace(roleClaim) && Enum.TryParse<TenantRole>(roleClaim, true, out var parsedRole))
        {
            return parsedRole;
        }

        return TenantRole.None;
    }

    private bool TryFetchTenantId(out Guid tenantId)
    {
        tenantId = Guid.Empty;
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return false;
        }

        if (httpContext.Items.TryGetValue(CoreConstants.MultiTenancy.TENANT_ID_ITEM_KEY, out var itemValue) &&
            itemValue is Guid keyTenantId && keyTenantId != Guid.Empty)
        {
            tenantId = keyTenantId;
            return true;
        }

        var tenantClaim = httpContext.User.FindFirst(CoreConstants.MultiTenancy.CLAIM_TENANT_ID)?.Value
                          ?? httpContext.User.FindFirst("tenant_id")?.Value;

        if (Guid.TryParse(tenantClaim, out tenantId) && tenantId != Guid.Empty)
        {
            return true;
        }

        if (httpContext.Request.Headers.TryGetValue(CoreConstants.MultiTenancy.HEADER_TENANT_ID, out var headerValue) &&
            Guid.TryParse(headerValue, out tenantId) && tenantId != Guid.Empty)
        {
            return true;
        }

        return false;
    }
}