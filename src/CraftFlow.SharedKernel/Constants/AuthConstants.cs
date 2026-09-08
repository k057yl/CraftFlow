namespace CraftFlow.SharedKernel.Constants;

public static class AuthConstants
{
    public const string DEFAULT_JWT_SECRET = "SUPER_SECRET_KEY_CRAFT_FLOW_2026_OLD_SCHULL_MUST_BE_LONG_ENOUGH";
    public const string JWT_SECRET_CONFIG_PATH = "Jwt:SecretKey";
    public const string DB_CONNECTION_STRING_PATH = "Database";

    public static class Headers
    {
        public const string SUBSCRIPTION_KEY = "X-Subscription-Key";
        public const string TELANT_ID = "X-Tenant-Id";
    }

    public static class Cache
    {
        public const string ACCESS_KEY_PREFIX = "access_key_hash_";
    }

    public static class Configuration
    {
        public const string API_KEY_SECURITY_SECTION = "ApiKeySecurity";
    }
}