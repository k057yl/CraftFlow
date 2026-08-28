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
    }

    public static class Production
    {
        public const string BATCH_NOT_FOUND = "PRODUCTION.BATCH_NOT_FOUND";
        public const string INSUFFICIENT_RAW_MATERIAL = "PRODUCTION.INSUFFICIENT_RAW_MATERIAL";
        public const string INVALID_STATUS_TRANSITION = "PRODUCTION.INVALID_STATUS_TRANSITION";
        public const string BATCH_NAME_REQUIRED = "PRODUCTION.BATCH_NAME_REQUIRED";
    }

    public static class Inventory
    {
        public const string STOCK_LOT_NEGATIVE_QUANTITY = "INVENTORY.STOCK_LOT_NEGATIVE_QUANTITY";
        public const string WAREHOUSE_NOT_FOUND = "INVENTORY.WAREHOUSE_NOT_FOUND";
        public const string WAREHOUSE_NAME_REQUIRED = "INVENTORY.WAREHOUSE_NAME_REQUIRED";
        public const string ITEM_NOT_FOUND = "INVENTORY.ITEM_NOT_FOUND";
    }

    public static class Catalog
    {
        public const string RECIPE_INVALID_TARGET_OUTPUT = "CATALOG.RECIPE_INVALID_TARGET_OUTPUT";
        public const string RECIPE_INVALID_INGREDIENT_QUANTITY = "CATALOG.RECIPE_INVALID_INGREDIENT_QUANTITY";
        public const string UNIT_OF_MEASURE_NOT_FOUND = "CATALOG.UNIT_OF_MEASURE_NOT_FOUND";
    }

    public static class Sales
    {
        public const string ORDER_NOT_FOUND = "SALES.ORDER_NOT_FOUND";
        public const string CUSTOMER_NOT_FOUND = "SALES.CUSTOMER_NOT_FOUND";
        public const string INSUFFICIENT_PRODUCT_STOCK = "SALES.INSUFFICIENT_PRODUCT_STOCK";
        public const string INVALID_ORDER_STATUS = "SALES.INVALID_ORDER_STATUS";
        public const string CUSTOMER_NAME_REQUIRED = "SALES.CUSTOMER_NAME_REQUIRED";
    }

    public static class UiMessages
    {
        public const string DATA_LOADED_SUCCESS = "UI.DATA_LOADED_SUCCESS";
        public const string DATA_LOAD_ERROR = "UI.DATA_LOAD_ERROR";
        public const string UOM_CREATED_SUCCESS = "UI.UOM_CREATED_SUCCESS";
        public const string RAW_MATERIAL_CREATED_SUCCESS = "UI.RAW_MATERIAL_CREATED_SUCCESS";
        public const string PRODUCT_CREATED_SUCCESS = "UI.PRODUCT_CREATED_SUCCESS";
        public const string RECIPE_CREATED_SUCCESS = "UI.RECIPE_CREATED_SUCCESS";
        public const string WAREHOUSE_CREATED_SUCCESS = "UI.WAREHOUSE_CREATED_SUCCESS";
        public const string STOCK_LOT_CREATED_SUCCESS = "UI.STOCK_LOT_CREATED_SUCCESS";
        public const string INVALID_INPUT_FIELDS = "UI.INVALID_INPUT_FIELDS";
        public const string API_ERROR_PREFIX = "UI.API_ERROR_PREFIX";
    }

    public static class Auth
    {
        public const string INVALID_CREDENTIALS = "AUTH.INVALID_CREDENTIALS";
        public const string USER_ALREADY_EXISTS = "AUTH.USER_ALREADY_EXISTS";
        public const string USER_NOT_FOUND = "AUTH.USER_NOT_FOUND";
        public const string TOKEN_GENERATION_FAILED = "AUTH.TOKEN_GENERATION_FAILED";
    }
}