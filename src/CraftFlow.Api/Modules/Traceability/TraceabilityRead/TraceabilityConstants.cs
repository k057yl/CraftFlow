using CraftFlow.Api.Common.Constants;

namespace CraftFlow.Api.Modules.Traceability.TraceabilityRead;
public static class TraceabilityConstants
{
    public const string SQL_GET_FORWARD_HEADER = $"""
        SELECT 
            sl.id AS RawMaterialStockLotId,
            sl.batch_number AS RawMaterialBatchNumber,
            COALESCE(rm.name, p.name, @DefaultMaterialName) AS RawMaterialName
        FROM {DbTables.STOCK_LOTS} sl
        LEFT JOIN {DbTables.RAW_MATERIALS} rm ON rm.id = sl.item_id
        LEFT JOIN {DbTables.PRODUCTS} p ON p.id = sl.item_id
        WHERE sl.id = @StockLotId AND sl.tenant_id = @TenantId;
        """;

    public const string SQL_GET_FORWARD_BATCHES = $"""
        SELECT 
            pb.id AS production_batch_id,
            pb.status AS batch_status_int,
            pb.created_date AS started_at,
            pb.completed_at AS completed_at,
            al.id AS aging_lot_id,
            al.batch_number AS aging_batch_number,
            ach.name AS chamber_name,
            al.status::text AS aging_status
        FROM {DbTables.CONSUMED_INGREDIENTS} ci
        JOIN {DbTables.PRODUCTION_BATCHES} pb ON pb.id = ci.production_batch_id
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_LOTS} al ON al.production_batch_id = pb.id
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_CHAMBERS} ach ON ach.id = al.aging_chamber_id
        WHERE ci.stock_lot_id = @StockLotId AND pb.tenant_id = @TenantId;
        """;

    public const string SQL_GET_BACKWARD_HEADER = $"""
        SELECT 
            so.id AS sales_order_id,
            c.name AS customer_name,
            sl.id AS product_stock_lot_id,
            sl.batch_number AS product_batch_number,
            p.name AS product_name,
            pb.id AS production_batch_id,
            pb.status AS batch_status_int,
            pb.created_date AS started_at,
            pb.completed_at AS completed_at
        FROM {DbTables.STOCK_LOTS} sl
        JOIN {DbTables.PRODUCTS} p ON p.id = sl.item_id
        LEFT JOIN {DbTables.SALES_ORDER_ITEMS} soi ON soi.product_id = p.id
        LEFT JOIN {DbTables.SALES_ORDERS} so ON so.id = soi.sales_order_id
        LEFT JOIN {DbTables.CUSTOMERS} c ON c.id = so.customer_id
        LEFT JOIN {DbTables.PRODUCTION_BATCHES} pb ON pb.id = sl.production_batch_id
        WHERE sl.id = @ProductStockLotId AND sl.tenant_id = @TenantId;
        """;

    public const string SQL_GET_BACKWARD_CONSUMED = $"""
        SELECT 
            rm_sl.id AS raw_material_stock_lot_id,
            rm.name AS raw_material_name,
            rm_sl.batch_number AS batch_number,
            ci.quantity AS quantity_used
        FROM {DbTables.CONSUMED_INGREDIENTS} ci
        JOIN {DbTables.STOCK_LOTS} rm_sl ON rm_sl.id = ci.stock_lot_id
        JOIN {DbTables.RAW_MATERIALS} rm ON rm.id = ci.raw_material_id
        WHERE ci.production_batch_id = @BatchId AND rm_sl.tenant_id = @TenantId;
        """;
}
