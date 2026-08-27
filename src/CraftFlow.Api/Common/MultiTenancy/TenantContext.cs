using CraftFlow.SharedKernel.Constants;

namespace CraftFlow.Api.Common.MultiTenancy
{
    public class TenantContext : ITenantContext
    {
        private const string TENANT_HEADER_NAME = "X-Tenant-Id";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid TenantId => FetchTenantId();
        public bool IsResolved => TryFetchTenantId(out _);

        private Guid FetchTenantId()
        {
            if (TryFetchTenantId(out var tenantId))
                return tenantId;

            throw new InvalidOperationException(ErrorCodes.General.INVALID_TENANT);
        }

        private bool TryFetchTenantId(out Guid tenantId)
        {
            tenantId = Guid.Empty;
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
                return false;

            if (httpContext.Request.Headers.TryGetValue(TENANT_HEADER_NAME, out var headerValue) &&
                Guid.TryParse(headerValue, out tenantId))
            {
                return true;
            }

            var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(tenantClaim, out tenantId))
            {
                return true;
            }

            return false;
        }
    }
}
