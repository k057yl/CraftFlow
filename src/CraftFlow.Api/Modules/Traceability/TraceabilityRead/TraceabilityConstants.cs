using CraftFlow.Api.Common.Constants;
using CraftFlow.SharedKernel.Constants;

namespace CraftFlow.Api.Modules.Traceability.TraceabilityRead;

public static class TraceabilityConstants
{
    public const string SQL_GET_FORWARD_HEADER = $"""
        SELECT 
            sl."Id" AS RawMaterialStockLotId,
            COALESCE(sl."BatchNumber", '{FormattingConstants.CONST_DEFAULT_BATCH_NUMBER}') AS RawMaterialBatchNumber,
            COALESCE(rm."Name", @DefaultMaterialName) AS RawMaterialName
        FROM {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS} sl
        INNER JOIN {DbSchemas.CATALOG}.{DbTables.RAW_MATERIALS} rm ON rm."Id" = sl."ItemId"
        WHERE sl."Id" = @StockLotId AND sl."TenantId" = @TenantId;
        """;

    public const string SQL_GET_FORWARD_BATCHES = $"""
        SELECT 
            pb."Id" AS production_batch_id,
            pb."State" AS batch_status_int,
            pb."StartedAt" AS started_at,
            pb."CompletedAt" AS completed_at,
            al."Id" AS aging_lot_id,
            al."BatchNumber" AS aging_batch_number,
            ach."Name" AS chamber_name,
            sloc."Name" AS location_name,
            al."State"::text AS aging_status
        FROM {DbSchemas.PRODUCTION}.{DbTables.CONSUMED_INGREDIENTS} ci
        JOIN {DbSchemas.PRODUCTION}.{DbTables.PRODUCTION_BATCHES} pb ON pb."Id" = ci."ProductionBatchId"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_LOTS} al ON al."ProductionBatchId" = pb."Id"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_CHAMBERS} ach ON ach."Id" = al."AgingChamberId"
        LEFT JOIN {DbSchemas.INVENTORY}.{DbTables.STORAGE_LOCATIONS} sloc ON sloc."Id" = al."StorageLocationId"
        WHERE ci."StockLotId" = @StockLotId AND pb."TenantId" = @TenantId;
        """;

    public const string SQL_GET_BACKWARD_HEADER = $"""
        SELECT 
            sl."Id" AS product_stock_lot_id,
            COALESCE(sl."BatchNumber", pb."Name") AS product_batch_number,
            p."Name" AS product_name,
            sl."Quantity" AS actual_quantity,
            sl."UnitPrice" AS unit_price,
            COALESCE(sl."ProductionBatchId", al."ProductionBatchId", pb."Id") AS production_batch_id,
            pb."State" AS batch_status_int,
            pb."StartedAt" AS started_at,
            pb."CompletedAt" AS completed_at,
            COALESCE(pb."PlannedOutputQuantity", 0) AS planned_quantity,
            COALESCE(pb."ActualOutputQuantity", 0) AS brew_output_quantity,
            COALESCE(EXTRACT(DAY FROM (COALESCE(al."ActualReleaseDate", NOW()) - al."PlacedAt"))::integer, 0) AS aging_days,
            ach."Name" AS chamber_name,
            sloc."Name" AS location_name,
            so."Id" AS sales_order_id,
            c."Name" AS customer_name
        FROM {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS} sl
        INNER JOIN {DbSchemas.CATALOG}.{DbTables.PRODUCTS} p ON p."Id" = sl."ItemId"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_LOTS} al ON al."ProductId" = p."Id" OR al."ProductionBatchId" = sl."ProductionBatchId"
        LEFT JOIN {DbSchemas.PRODUCTION}.{DbTables.PRODUCTION_BATCHES} pb ON pb."Id" = COALESCE(sl."ProductionBatchId", al."ProductionBatchId")
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_CHAMBERS} ach ON ach."Id" = al."AgingChamberId"
        LEFT JOIN {DbSchemas.INVENTORY}.{DbTables.STORAGE_LOCATIONS} sloc ON sloc."Id" = al."StorageLocationId"
        LEFT JOIN {DbSchemas.SALES}.{DbTables.SALES_ORDER_ITEMS} soi ON soi."StockLotId" = sl."Id"
        LEFT JOIN {DbSchemas.SALES}.{DbTables.SALES_ORDERS} so ON so."Id" = soi."SalesOrderId"
        LEFT JOIN {DbSchemas.SALES}.{DbTables.CUSTOMERS} c ON c."Id" = so."CustomerId"
        WHERE sl."Id" = @ProductStockLotId AND sl."TenantId" = @TenantId
        ORDER BY al."PlacedAt" DESC NULLS LAST
        LIMIT 1;
        """;

    public const string SQL_GET_BACKWARD_CONSUMED = $"""
        SELECT 
            COALESCE(rm_sl."Id", ci."StockLotId") AS raw_material_stock_lot_id,
            COALESCE(rm."Name", '{FormattingConstants.CONST_DEFAULT_MATERIAL_NAME}') AS raw_material_name,
            COALESCE(rm_sl."BatchNumber", '{FormattingConstants.CONST_DEFAULT_BATCH_NUMBER}') AS batch_number,
            ci."Quantity" AS quantity_used,
            COALESCE(uom."Code", '') AS unit_of_measure,
            COALESCE(sup."Name", '{FormattingConstants.NOT_AVAILABLE}') AS supplier_name
        FROM {DbSchemas.PRODUCTION}.{DbTables.CONSUMED_INGREDIENTS} ci
        LEFT JOIN {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS} rm_sl ON rm_sl."Id" = ci."StockLotId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.RAW_MATERIALS} rm ON rm."Id" = ci."RawMaterialId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.UNITS_OF_MEASURE} uom ON uom."Id" = rm."UnitOfMeasureId"
        LEFT JOIN {DbSchemas.PROCUREMENT}.{DbTables.PURCHASE_ORDER_ITEMS} poi ON poi."RawMaterialId" = rm."Id"
        LEFT JOIN {DbSchemas.PROCUREMENT}.{DbTables.PURCHASE_ORDERS} po ON po."Id" = poi."PurchaseOrderId"
        LEFT JOIN {DbSchemas.PROCUREMENT}.{DbTables.SUPPLIERS} sup ON sup."Id" = po."SupplierId"
        WHERE ci."ProductionBatchId" = @BatchId;
        """;
}