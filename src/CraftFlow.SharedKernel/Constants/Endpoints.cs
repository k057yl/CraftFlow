namespace CraftFlow.SharedKernel.Constants;

public static class Endpoints
{
    public const string UOM = "api/catalog/units-of-measure";
    public const string RAW_MATERIALS = "api/catalog/raw-materials";
    public const string PRODUCTS = "api/catalog/products";
    public const string RECIPES = "api/catalog/recipes";
    public const string WAREHOUSES = "api/inventory/warehouses";
    public const string STOCK_LOTS = "api/inventory/stock-lots";
    public const string BATCHES_START = "api/production/batches/start";
    public const string BATCHES_ACTIVE = "api/production/batches/active";
    public const string BATCHES_COMPLETE = "api/production/batches/complete";
    public const string BATCHES_CONSUME = "api/production/batches/consume-ingredient";
    public const string CUSTOMERS = "api/sales/customers";
    public const string ORDERS_SHIP = "api/sales/orders/ship";
    public const string DASHBOARD = "api/analytics/dashboard";
    public const string REGISTER = "api/identity/register";
    public const string LOGIN = "api/identity/login";
    public const string AUDIT_LOGS = "api/analytics/audit-logs";
    public const string PRODUCTION_COSTING = "api/production/costing";

    // Procurement
    public const string SUPPLIERS = "api/procurement/suppliers";
    public const string PURCHASE_ORDERS = "api/procurement/purchase-orders";
    public const string PURCHASE_ORDERS_RECEIVE = "api/procurement/purchase-orders/receive";

    // Aging
    public const string AGING_CHAMBERS = "api/inventory/aging-chambers";
    public const string AGING_LOTS_ACTIVE = "api/aging/lots/active";
    public const string AGING_LOTS_TRANSFER = "api/aging/lots/transfer";
    public const string AGING_LOTS_RELEASE = "api/aging/lots/release";

    // Traceability & MRP
    public const string TRACEABILITY_FORWARD = "api/traceability/forward";
    public const string TRACEABILITY_BACKWARD = "api/traceability/backward";
    public const string MRP_REQUIREMENTS = "api/mrp/requirements";

    public const string BATCHES_READY_AGING = "api/production/batches/ready-for-aging";
    public const string BATCHES_DISCARD = "api/production/batches/discard";
    public const string CALCULATE_REQUIREMENTS = "api/production/calculate-requirements";
    public const string ESTIMATE_COST = "api/production/estimate-cost";
}