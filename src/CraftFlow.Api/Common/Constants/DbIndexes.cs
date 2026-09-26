namespace CraftFlow.Api.Common.Constants;

public static class DbIndexes
{
    public static class Production
    {
        public const string IX_PRODUCTION_BATCHES_TIMER_MONITORING = "IX_PRODUCTION_BATCHES_TIMER_MONITORING";
        public const string IX_CONSUMED_INGREDIENTS_BATCH = "IX_CONSUMED_INGREDIENTS_BATCH";
    }

    public static class Identity
    {
        public const string IX_USERS_TENANT_EMAIL = "IX_USERS_TENANT_EMAIL";
        public const string IX_USERS_TENANT_ROLE_ACTIVE = "IX_USERS_TENANT_ROLE_ACTIVE";
        public const string IX_USERS_EMAIL = "IX_USERS_EMAIL";
        public const string IX_ORGANIZATIONS_RETENTION_CHECK = "IX_ORGANIZATIONS_RETENTION_CHECK";
        public const string IX_USERS_OTP_EXPIRATION = "IX_USERS_OTP_EXPIRATION";
    }

    public static class Inventory
    {
        public const string IX_STOCK_LOTS_LOOKUP = "IX_STOCK_LOTS_LOOKUP";
    }
    
    public static class Aging
    {
        public const string IX_AGING_CHAMBERS_TENANT_NAME = "IX_AGING_CHAMBERS_TENANT_NAME";
        public const string IX_AGING_LOTS_CHAMBER = "IX_AGING_LOTS_CHAMBER";
        public const string IX_AGING_LOTS_BATCH = "IX_AGING_LOTS_BATCH";
        public const string IX_AGING_LOT_ITEMS_LOT = "IX_AGING_LOT_ITEMS_LOT";
    }

    public static class Procurement
    {
        public const string IX_SUPPLIERS_TENANT_NAME = "IX_SUPPLIERS_TENANT_NAME";
        public const string IX_PURCHASE_ORDERS_SUPPLIER = "IX_PURCHASE_ORDERS_SUPPLIER";
        public const string IX_PURCHASE_ORDERS_STATUS = "IX_PURCHASE_ORDERS_STATUS";
        public const string IX_PURCHASE_ORDER_ITEMS_ORDER = "IX_PURCHASE_ORDER_ITEMS_ORDER";
    }

    public static class Sales
    {
        public const string IX_CUSTOMERS_TENANT_NAME = "IX_CUSTOMERS_TENANT_NAME";
        public const string IX_SALES_ORDERS_CUSTOMER = "IX_SALES_ORDERS_CUSTOMER";
        public const string IX_SALES_ORDERS_STATUS = "IX_SALES_ORDERS_STATUS";
        public const string IX_SALES_ORDER_ITEMS_ORDER = "IX_SALES_ORDER_ITEMS_ORDER";
    }

    public static class Subscriptions
    {
        public const string IX_ACCESS_KEYS_HASH = "IX_ACCESS_KEYS_HASH";
        public const string IX_SUBSCRIPTION_PLANS_CODE = "IX_SUBSCRIPTION_PLANS_CODE";
        public const string IX_ACCESS_KEYS_TENANT = "IX_ACCESS_KEYS_TENANT";
    }
}