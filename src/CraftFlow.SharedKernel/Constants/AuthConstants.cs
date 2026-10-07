namespace CraftFlow.SharedKernel.Constants;

public static class AuthConstants
{
    public const string DEFAULT_JWT_SECRET = "SUPER_SECRET_KEY_CRAFT_FLOW_2026_OLD_SCHULL_MUST_BE_LONG_ENOUGH";
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

    public static class ConfigurationKeys
    {
        public const string JWT_SECRET_KEY_PATH = "Jwt:SecretKey";
        public const string JWT_SECRET_KEY_ENV = "JWT_SECRET_KEY";
        public const string JWT_ISSUER_PATH = "Jwt:Issuer";
        public const string JWT_AUDIENCE_PATH = "Jwt:Audience";
    }

    public static class Roles
    {
        public const string ADMIN = "Admin";
        public const string USER = "User";
    }

    public static class AdminEnvironment
    {
        public const string ADMIN_EMAIL = "ADMIN_EMAIL";
        public const string ADMIN_PASSWORD = "ADMIN_PASSWORD";
        public const string ADMIN_NAME = "ADMIN_NAME";
        public const string DEFAULT_ADMIN_NAME = "System Admin";
    }

    public static class Claims
    {
        public const string EMAIL = "email";
        public const string ROLE_SHORT = "role";
        public const string ROLE_FULL = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        public const string ROLE_ID = "role_id";
        public const string TENANT_ID = "tenant_id";
        public const string FULL_NAME = "full_name";
    }

    public static class Token
    {
        public const int DEFAULT_EXPIRATION_DAYS = 7;
    }

    public static class ErrorMessages
    {
        public const string JWT_SECRET_KEY_NOT_CONFIGURED = "JWT_SECRET_KEY_NOT_CONFIGURED";
    }

    public static class OTP
    {
        public const string FORMAT_D6 = "D6";
        public const int EXPIRATION_MINUTES = 5;
    }
}