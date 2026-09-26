namespace CraftFlow.Api.Common.Constants;

public static class DbIndexes
{
    public static class Production
    {
        public const string IX_PRODUCTION_BATCHES_TIMER_MONITORING = "IX_PRODUCTION_BATCHES_TIMER_MONITORING";
    }

    public static class Identity
    {
        public const string IX_USERS_TENANT_EMAIL = "IX_USERS_TENANT_EMAIL";
        public const string IX_USERS_TENANT_ROLE_ACTIVE = "IX_USERS_TENANT_ROLE_ACTIVE";
        public const string IX_USERS_EMAIL = "IX_USERS_EMAIL";
    }
}