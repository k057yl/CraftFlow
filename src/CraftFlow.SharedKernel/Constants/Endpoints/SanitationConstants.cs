namespace CraftFlow.SharedKernel.Constants.Endpoints;

public static class SanitationConstants
{
    public const string BASE = "api/sanitation";
    public const string RULES = "api/sanitation/rules";
    public const string TASKS = "api/sanitation/tasks";
    public const string TASKS_SCHEDULE = "api/sanitation/tasks/schedule";
    public const string TASKS_VERIFY = "api/sanitation/tasks/verify";
    public const string WAREHOUSE_HISTORY = "api/sanitation/warehouses/{warehouseId:guid}/history";
    public const string WAREHOUSE_DETAILS = "api/sanitation/details/{targetType}/{targetId:guid}";
}