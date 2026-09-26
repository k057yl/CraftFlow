namespace CraftFlow.Api.Common.Constants;
public static class CoreConstants
{
    public static class MultiTenancy
    {
        public const string HEADER_TENANT_ID = "X-Tenant-Id";
        public const string CLAIM_TENANT_ID = "tenant_id";
        public const string TENANT_ID_ITEM_KEY = "TenantId";
        public const string DEFAULT_TENANT_ID_STRING = "00000000-0000-0000-0000-000000000001";
    }

    public static class Quotas
    {
        public const int MAX_FREE_MONTHLY_BATCHES = 4;
    }

    public static class InventoryThresholds
    {
        public const decimal LOW_STOCK_MIN_QUANTITY = 50.0m;
        public const int MONITOR_INTERVAL_MINUTES = 5;
    }

    public static class Audit
    {
        public const string SYSTEM_USER_ID_STRING = "00000000-0000-0000-0000-000000000000";
        public const string AUDIT_CHANGE_FORMAT = "Entity {0} changed.";
    }

    public static class ApiServiceConstants
    {
        public const string API_BASE_URL = "https://localhost:7203/";
        public const string BEARER_SCHEME = "Bearer";
    }

    public static class Billing
    {
        public const string PLAN_FREE_CODE = "FREE";
    }
}
