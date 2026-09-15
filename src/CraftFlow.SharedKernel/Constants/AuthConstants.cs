namespace CraftFlow.SharedKernel.Constants;

public static class AuthConstants
{
    public const string DEFAULT_JWT_SECRET = "SUPER_SECRET_KEY_CRAFT_FLOW_2026_OLD_SCHULL_MUST_BE_LONG_ENOUGH";
    public const string JWT_SECRET_CONFIG_PATH = "Jwt:SecretKey";
    public const string JWT_SECRET_KEY_ENV = "JWT_SECRET_KEY";
    public const string DB_CONNECTION_STRING_PATH = "Database";

    public static class Headers
    {
        public const string SUBSCRIPTION_KEY = "X-Subscription-Key";
        public const string TENANT_ID = "X-Tenant-Id";
    }

    public static class Cache
    {
        public const string ACCESS_KEY_PREFIX = "access_key_hash_";
        public const string KEY_LAST_USED_PREFIX = "key_last_used_";
        public const int KEY_EXPIRATION_MINUTES = 10;
        public const int KEY_LAST_USED_HOURS = 2;
    }

    public static class Configuration
    {
        public const string API_KEY_SECURITY_SECTION = "ApiKeySecurity";
    }

    public static class Roles
    {
        public const string ADMIN = "Admin";
        public const string USER = "User";
    }

    public static class Claims
    {
        public const string EMAIL = "email";
        public const string ROLE_SHORT = "role";
        public const string ROLE_FULL = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        public const string TENANT_ID = "tenant_id";
        public const string FULL_NAME = "full_name";
    }

    public static class ADMIN_CONFIG_KEYS
    {
        public const string ADMIN_EMAIL_KEY = "ADMIN_EMAIL";
        public const string ADMIN_PASSWORD_KEY = "ADMIN_PASSWORD";
        public const string ADMIN_NAME_KEY = "ADMIN_NAME";
    }

    public static class SYSTEM_SECURITY_CONSTANTS
    {
        public const string ADMIN_ROLE_CLAIM_KEY = "SYSTEM_ADMIN_ROLE";
        public const string ADMIN_ROLE_VALUE = "BIG_BOSS";
    }

    public static class OTP
    {
        public const string FORMAT_D6 = "D6";
        public const int EXPIRATION_MINUTES = 5;
    }
}