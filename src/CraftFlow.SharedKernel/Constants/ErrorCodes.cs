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
    }

    public static class Inventory
    {
        public const string STOCK_LOT_NEGATIVE_QUANTITY = "INVENTORY.STOCK_LOT_NEGATIVE_QUANTITY";
        public const string WAREHOUSE_NOT_FOUND = "INVENTORY.WAREHOUSE_NOT_FOUND";
    }
}