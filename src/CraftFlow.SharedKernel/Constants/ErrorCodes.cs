namespace CraftFlow.SharedKernel.Constants;

public static class ErrorCodes
{
    public static class General
    {
        public const string NOT_FOUND = "GENERAL.NOT_FOUND";
        public const string VALUE_REQUIRED = "GENERAL.VALUE_REQUIRED";
        public const string INVALID_TENANT = "GENERAL.INVALID_TENANT";
        public const string UNAUTHORIZED = "GENERAL.UNAUTHORIZED";
        public const string NULL_VALUE = "GENERAL.NULL_VALUE";
        public const string INVALID_FORMAT = "GENERAL.INVALID_FORMAT";
        public const string ALREADY_EXISTS = "GENERAL.ALREADY_EXISTS";
        public const string VALIDATION_ERROR = "GENERAL.VALIDATION_ERROR";
    }

    public static class Production
    {
        public const string BATCH_NOT_FOUND = "PRODUCTION.BATCH_NOT_FOUND";
        public const string INSUFFICIENT_RAW_MATERIAL = "PRODUCTION.INSUFFICIENT_RAW_MATERIAL";
        public const string INVALID_STATUS_TRANSITION = "PRODUCTION.INVALID_STATUS_TRANSITION";
        public const string BATCH_NAME_REQUIRED = "PRODUCTION.BATCH_NAME_REQUIRED";
        public const string CONSUMED_INGREDIENT_NEGATIVE_QUANTITY = "PRODUCTION.CONSUMED_INGREDIENT_NEGATIVE_QUANTITY";
        public const string ONLY_COMPLETED_BATCHES_CAN_BE_TRANSFERRED_TO_AGING = "PRODUCTION.ONLY_COMPLETED_BATCHES_CAN_BE_TRANSFERRED_TO_AGING";
        public const string EXPIRATION_DATE_MUST_BE_IN_FUTURE = "PRODUCTION.EXPIRATION_DATE_MUST_BE_IN_FUTURE";
    }

    public static class Inventory
    {
        public const string STOCK_LOT_NEGATIVE_QUANTITY = "INVENTORY.STOCK_LOT_NEGATIVE_QUANTITY";
        public const string UNIT_LOT_NEGATIVE_QUANTITY = "INVENTORY.UNIT_LOT_NEGATIVE_QUANTITY";
        public const string WAREHOUSE_NOT_FOUND = "INVENTORY.WAREHOUSE_NOT_FOUND";
        public const string WAREHOUSE_NAME_REQUIRED = "INVENTORY.WAREHOUSE_NAME_REQUIRED";
        public const string ITEM_NOT_FOUND = "INVENTORY.ITEM_NOT_FOUND";
    }

    public static class Catalog
    {
        public const string RECIPE_INVALID_TARGET_OUTPUT = "CATALOG.RECIPE_INVALID_TARGET_OUTPUT";
        public const string RECIPE_INVALID_INGREDIENT_QUANTITY = "CATALOG.RECIPE_INVALID_INGREDIENT_QUANTITY";
        public const string UNIT_OF_MEASURE_NOT_FOUND = "CATALOG.UNIT_OF_MEASURE_NOT_FOUND";
        public const string RECIPE_INVALID_AGING_DAYS = "CATALOG.RECIPE_INVALID_AGING_DAYS";
        public const string PRODUCT_NOT_FOUND = "CATALOG.PRODUCT_NOT_FOUND";
    }

    public static class Sales
    {
        public const string ORDER_NOT_FOUND = "SALES.ORDER_NOT_FOUND";
        public const string CUSTOMER_NOT_FOUND = "SALES.CUSTOMER_NOT_FOUND";
        public const string INSUFFICIENT_PRODUCT_STOCK = "SALES.INSUFFICIENT_PRODUCT_STOCK";
        public const string INVALID_ORDER_STATUS = "SALES.INVALID_ORDER_STATUS";
        public const string CUSTOMER_NAME_REQUIRED = "SALES.CUSTOMER_NAME_REQUIRED";
    }

    public static class Auth
    {
        public const string INVALID_CREDENTIALS = "AUTH.INVALID_CREDENTIALS";
        public const string USER_ALREADY_EXISTS = "AUTH.USER_ALREADY_EXISTS";
        public const string USER_NOT_FOUND = "AUTH.USER_NOT_FOUND";
        public const string TOKEN_GENERATION_FAILED = "AUTH.TOKEN_GENERATION_FAILED";
        public const string INVALID_ACCESS_KEY = "AUTH.INVALID_ACCESS_KEY";
        public const string KEY_REVOKED = "AUTH.KEY_REVOKED";
        public const string ACCOUNT_NOT_ACTIVATED = "AUTH.ACCOUNT_NOT_ACTIVATED";
        public const string OTP_EXPIRED = "AUTH.OTP_EXPIRED";
        public const string ALREADY_ACTIVATED = "AUTH.ALREADY_ACTIVATED";
    }

    public static class Notifications
    {
        public const string LOW_STOCK_ALERT = "NOTIFICATIONS.LOW_STOCK_ALERT";
        public const string COST_CALCULATED = "NOTIFICATIONS.COST_CALCULATED";
    }

    public static class Procurement
    {
        public const string SUPPLIER_NAME_REQUIRED = "PROCUREMENT.SUPPLIER_NAME_REQUIRED";
        public const string SUPPLIER_NOT_FOUND = "PROCUREMENT.SUPPLIER_NOT_FOUND";
        public const string PURCHASE_ORDER_NOT_FOUND = "PROCUREMENT.PURCHASE_ORDER_NOT_FOUND";
        public const string PURCHASE_ORDER_INVALID_STATUS = "PROCUREMENT.PURCHASE_ORDER_INVALID_STATUS";
        public const string PURCHASE_ORDER_ITEM_INVALID_QUANTITY = "PROCUREMENT.PURCHASE_ORDER_ITEM_INVALID_QUANTITY";
        public const string PURCHASE_ORDER_ITEM_INVALID_PRICE = "PROCUREMENT.PURCHASE_ORDER_ITEM_INVALID_PRICE";
    }

    public static class Aging
    {
        public const string CHAMBER_NOT_FOUND = "AGING.CHAMBER_NOT_FOUND";
        public const string CHAMBER_NAME_REQUIRED = "AGING.CHAMBER_NAME_REQUIRED";
        public const string INVALID_LOT_STATE = "AGING.INVALID_LOT_STATE";
        public const string INSUFFICIENT_AGING_CAPACITY = "AGING.INSUFFICIENT_AGING_CAPACITY";
    }

    public static class Saas
    {
        public const string QUOTA_EXCEEDED = "SAAS.QUOTA_EXCEEDED";
        public const string SUBSCRIPTION_EXPIRED = "SAAS.SUBSCRIPTION_EXPIRED";
        public const string TENANT_MISMATCH = "SAAS.TENANT_MISMATCH";
    }

    public static class Supplier
    {
        public const string PHONE_NUMBER_INVALID_FORMAT = "SUPPLIER.PHONE_NUMBER_INVALID_FORMAT";
    }
}