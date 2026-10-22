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
        public const string MAX_LENGTH_EXCEEDED = "GENERAL.MAX_LENGTH_EXCEEDED";
        public const string INVALID_EMAIL_FORMAT = "GENERAL.INVALID_EMAIL_FORMAT";
        public const string INVALID_AMOUNT = "GENERAL.INVALID_AMOUNT";
    }

    public static class Tenant
    {
        public const string INVALID_TENANT_ID = "TENANT_INVALID_ID";
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
        public const string BATCH_CANNOT_BE_DELETED = "PRODUCTION_BATCH_CANNOT_BE_DELETED";
    }

    public static class Inventory
    {
        public const string STOCK_LOT_NEGATIVE_QUANTITY = "INVENTORY.STOCK_LOT_NEGATIVE_QUANTITY";
        public const string UNIT_LOT_NEGATIVE_QUANTITY = "INVENTORY.UNIT_LOT_NEGATIVE_QUANTITY";
        public const string WAREHOUSE_NOT_FOUND = "INVENTORY.WAREHOUSE_NOT_FOUND";
        public const string WAREHOUSE_NAME_REQUIRED = "INVENTORY.WAREHOUSE_NAME_REQUIRED";
        public const string ITEM_NOT_FOUND = "INVENTORY.ITEM_NOT_FOUND";
        public const string LOCATION_NAME_REQUIRED = "INVENTORY.LOCATION_NAME_REQUIRED";
        public const string LOCATION_TYPE_REQUIRED = "INVENTORY.LOCATION_TYPE_REQUIRED";
        public const string INVALID_CAPACITY = "INVENTORY.INVALID_CAPACITY";
        public const string PARENT_CONTAINER_REQUIRED = "INVENTORY.PARENT_CONTAINER_REQUIRED";
        public const string STOCK_LOT_NOT_FOUND = "INVENTORY.STOCK_LOT_NOT_FOUND";
        public const string STOCK_LOT_NEGATIVE_PRICE = "INV_STOCK_LOT_NEGATIVE_PRICE";
        public const string LOCATION_NOT_FOUND = "INVENTORY.LOCATION_NOT_FOUND";
        public const string LOCATION_NOT_AVAILABLE = "INVENTORY.LOCATION_NOT_AVAILABLE";
        public const string LOCATION_NOT_EMPTY_FOR_SANITATION = "INVENTORY.LOCATION_NOT_EMPTY_FOR_SANITATION";
        public const string CAPACITY_EXCEEDED = "INVENTORY.CAPACITY_EXCEEDED";
        public const string LOCATION_HAS_VOLUME = "INV_LOCATION_HAS_VOLUME";
        public const string AMBIGUOUS_PARENT_CONTAINER = "INV_AMBIGUOUS_PARENT_CONTAINER";
        public const string INVALID_ALLOCATED_QUANTITY = "INV_INVALID_ALLOCATED_QUANTITY";
        public const string ALLOCATED_QUANTITY_EXCEEDS_TOTAL = "INV_ALLOCATED_QUANTITY_EXCEEDS_TOTAL";
        public const string INVALID_PRODUCTION_BATCH_ID = "INVENTORY.INVALID_PRODUCTION_BATCH_ID";
    }

    public static class Catalog
    {
        public const string RECIPE_INVALID_TARGET_OUTPUT = "CATALOG.RECIPE_INVALID_TARGET_OUTPUT";
        public const string RECIPE_INVALID_INGREDIENT_QUANTITY = "CATALOG.RECIPE_INVALID_INGREDIENT_QUANTITY";
        public const string UNIT_OF_MEASURE_NOT_FOUND = "CATALOG.UNIT_OF_MEASURE_NOT_FOUND";
        public const string RECIPE_INVALID_AGING_DAYS = "CATALOG.RECIPE_INVALID_AGING_DAYS";
        public const string PRODUCT_NOT_FOUND = "CATALOG.PRODUCT_NOT_FOUND";
        public const string RECIPE_NOT_FOUND = "CATALOG.RECIPE_NOT_FOUND";
        public const string UOM_INVALID_NAME = "CATALOG.UOM_INVALID_NAME";
        public const string UOM_INVALID_CODE = "CATALOG.UOM_INVALID_CODE";
        public const string UOM_INVALID_TYPE = "CATALOG.UOM_INVALID_TYPE";
        public const string UOM_INVALID_CONVERSION_FACTOR = "CATALOG.UOM_INVALID_CONVERSION_FACTOR";
        public const string UOM_INVALID_BASE_UNIT_ID = "CATALOG.UOM_INVALID_BASE_UNIT_ID";
        public const string UOM_BASE_UNIT_FACTOR_MUST_BE_ONE = "CATALOG.UOM_BASE_UNIT_FACTOR_MUST_BE_ONE";
        public const string UOM_SOURCE_NULL = "CATALOG.UOM_SOURCE_NULL";
        public const string UOM_TARGET_NULL = "CATALOG.UOM_TARGET_NULL";
        public const string UOM_TYPE_MISMATCH = "CATALOG.UOM_TYPE_MISMATCH";
        public const string RAW_MATERIAL_HAS_STOCK = "CATALOG.RAW_MATERIAL_HAS_STOCK";
        public const string RAW_MATERIAL_IN_ACTIVE_RECIPE = "CATALOG.RAW_MATERIAL_IN_ACTIVE_RECIPE";
        public const string RECIPE_HAS_ACTIVE_BATCHES = "CATALOG.RECIPE_HAS_ACTIVE_BATCHES";
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
        public const string ACCESS_DENIED = "AUTH.ACCESS_DENIED";
        public const string UNAUTHORIZED = "AUTH.UNAUTHORIZED";
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
        public const string DISCARD_REASON_MANUAL_DISCARD = "AGING.DISCARD_REASON_MANUAL_DISCARD";
        public const string DISCARD_REASON_INVENTORY_LOSS = "AGING.DISCARD_REASON_INVENTORY_LOSS";
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

    public static class Sanitation
    {
        public const string CHEMICAL_REQUIRED = "SANITATION.CHEMICAL_REQUIRED";
        public const string TASK_NOT_FOUND = "SANITATION.TASK_NOT_FOUND";
        public const string RULE_NOT_FOUND = "SANITATION.RULE_NOT_FOUND";
        public const string INVALID_STATUS_TRANSITION = "SANITATION.INVALID_STATUS_TRANSITION";
        public const string PERFORMER_REQUIRED = "SANITATION.PERFORMER_REQUIRED";
        public const string VERIFIER_REQUIRED = "SANITATION.VERIFIER_REQUIRED";
        public const string INVALID_DURATION = "SANITATION.INVALID_DURATION";
        public const string TARGET_ID_REQUIRED = "SANITATION.TARGET_ID_REQUIRED";
        public const string INVALID_TRIGGER_CONFIG = "SANITATION.INVALID_TRIGGER_CONFIG";
        public const string TASK_NOT_COMPLETED = "SANITATION.TASK_NOT_COMPLETED";
    }
}