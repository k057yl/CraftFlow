namespace CraftFlow.Api.Core.Constants;

public static class DbTables
{
    // --- IDENTITY (schema: identity) ---
    public const string USERS = "users";
    public const string SYSTEM_ADMINS = "system_admins";
    public const string ORGANIZATIONS = "organizations";
    public const string ORGANIZATION_MEMBERS = "organization_members";
    public const string AUDIT_LOGS = "audit_logs";

    // --- CATALOG (schema: catalog) ---
    public const string UNITS_OF_MEASURE = "units_of_measure";
    public const string RAW_MATERIALS = "raw_materials";
    public const string PRODUCTS = "products";
    public const string RECIPES = "recipes";
    public const string RECIPE_INGREDIENTS = "recipe_ingredients";

    // --- INVENTORY (schema: inventory) ---
    public const string WAREHOUSES = "warehouses";
    public const string STORAGE_LOCATIONS = "storage_locations";
    public const string STOCK_LOTS = "stock_lots";
    public const string STOCK_LOT_STORAGE_LOCATIONS = "stock_lot_storage_locations";

    // --- PRODUCTION (schema: production) ---
    public const string PRODUCTION_BATCHES = "production_batches";
    public const string CONSUMED_INGREDIENTS = "consumed_ingredients";

    // --- PROCUREMENT (schema: procurement) ---
    public const string SUPPLIERS = "suppliers";
    public const string PURCHASE_ORDERS = "purchase_orders";
    public const string PURCHASE_ORDER_ITEMS = "purchase_order_items";

    // --- SALES (schema: sales) ---
    public const string CUSTOMERS = "customers";
    public const string SALES_ORDERS = "sales_orders";
    public const string SALES_ORDER_ITEMS = "sales_order_items";

    // --- SAAS / SUBSCRIPTIONS (schema: saas) ---
    public const string SUBSCRIPTION_PLANS = "subscription_plans";
    public const string TENANT_SUBSCRIPTIONS = "tenant_subscriptions";
    public const string SUBSCRIPTION_PAYMENTS = "subscription_payments";
    public const string SUBSCRIPTION_INVOICES = "subscription_invoices";

    // --- SANITATION (schema: sanitation) ---
    public const string SANITATION_RULES = "sanitation_rules";
    public const string SANITATION_TASKS = "sanitation_tasks";
    public const string SANITATION_LOGS = "sanitation_logs";

    // --- VERTICAL MODULE: CHEESE MAKING (schema: cheese_making) ---
    public const string AGING_CHAMBERS = "aging_chambers";
    public const string AGING_LOTS = "aging_lots";
    public const string AGING_LOT_ITEMS = "aging_lot_items";

    // --- VERTICAL MODULES (Future Tables for Brewery, Meat, Fermentation) ---
}