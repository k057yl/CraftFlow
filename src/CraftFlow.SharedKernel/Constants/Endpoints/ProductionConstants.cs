namespace CraftFlow.Api.Modules.Production;
public class ProductionConstants
{
    public const string BATCHES_START = "api/production/batches/start";
    public const string BATCHES_ACTIVE_SUMMARY = "api/production/batches/active-summary";
    public const string BATCHES_COMPLETE = "api/production/batches/complete";
    public const string BATCHES_CONSUME = "api/production/batches/consume-ingredient";
    public const string BATCHES_DISCARD = "api/production/batches/discard";
    public const string BATCHES_READY_AGING = "api/production/batches/ready-for-aging";
    public const string PRODUCTION_COSTING = "api/production/costing";
    public const string CALCULATE_REQUIREMENTS = "api/production/calculate-requirements";
    public const string ESTIMATE_COST = "api/production/estimate-cost";
    public const string CALCULATE_MAX_OUTPUT = "api/production/calculate-max-output";
}