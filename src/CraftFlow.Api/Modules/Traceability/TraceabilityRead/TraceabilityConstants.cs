using CraftFlow.Api.Common.Constants;

namespace CraftFlow.Api.Modules.Traceability.TraceabilityRead;

public static class TraceabilityConstants
{
    // 1. Заголовок сырья для Forward Traceability
    public const string SQL_GET_FORWARD_HEADER = $"""
        SELECT 
            sl."Id" AS RawMaterialStockLotId,
            COALESCE(sl."BatchNumber", 'Б/Н') AS RawMaterialBatchNumber,
            COALESCE(rm."Name", p."Name", @DefaultMaterialName) AS RawMaterialName
        FROM {DbTables.STOCK_LOTS} sl
        LEFT JOIN {DbTables.RAW_MATERIALS} rm ON rm."Id" = sl."ItemId"
        LEFT JOIN {DbTables.PRODUCTS} p ON p."Id" = sl."ItemId"
        WHERE sl."Id" = @StockLotId AND sl."TenantId" = @TenantId;
        """;

    // 2. Все варки и камеры созревания, где использовался этот StockLot сырья
    public const string SQL_GET_FORWARD_BATCHES = $"""
        SELECT 
            pb."Id" AS production_batch_id,
            pb."Status" AS batch_status_int,
            pb."StartedAt" AS started_at,
            pb."CompletedAt" AS completed_at,
            al."Id" AS aging_lot_id,
            al."BatchNumber" AS aging_batch_number,
            ach."Name" AS chamber_name,
            al."State"::text AS aging_status
        FROM {DbTables.CONSUMED_INGREDIENTS} ci
        JOIN {DbTables.PRODUCTION_BATCHES} pb ON pb."Id" = ci."ProductionBatchId"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_LOTS} al ON al."ProductionBatchId" = pb."Id"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_CHAMBERS} ach ON ach."Id" = al."AgingChamberId"
        WHERE ci."StockLotId" = @StockLotId AND pb."TenantId" = @TenantId;
        """;

    // 3. Заголовок готовой продукции и варка-источник для Backward Traceability
    public const string SQL_GET_BACKWARD_HEADER = $"""
        SELECT 
            NULL::uuid AS sales_order_id,
            'На складе' AS customer_name,
            COALESCE(sl."Id", pb."Id") AS product_stock_lot_id,
            COALESCE(sl."BatchNumber", pb."Name") AS product_batch_number,
            p."Name" AS product_name,
            pb."Id" AS production_batch_id,
            pb."Status" AS batch_status_int,
            pb."StartedAt" AS started_at,
            pb."CompletedAt" AS completed_at
        FROM {DbTables.PRODUCTION_BATCHES} pb
        INNER JOIN {DbTables.PRODUCTS} p ON p."Id" = pb."TargetProductId"
        LEFT JOIN {DbTables.STOCK_LOTS} sl ON sl."ProductionBatchId" = pb."Id"
        WHERE (pb."Id" = @ProductStockLotId OR sl."Id" = @ProductStockLotId) 
          AND pb."TenantId" = @TenantId
        LIMIT 1;
        """;

    // 4. Все ингредиенты (сырье), задействованные в этой варке
    public const string SQL_GET_BACKWARD_CONSUMED = $"""
        SELECT 
            COALESCE(rm_sl."Id", ci."StockLotId") AS raw_material_stock_lot_id,
            COALESCE(rm."Name", 'Сырье') AS raw_material_name,
            COALESCE(rm_sl."BatchNumber", 'Б/Н') AS batch_number,
            ci."Quantity" AS quantity_used
        FROM {DbTables.CONSUMED_INGREDIENTS} ci
        LEFT JOIN {DbTables.STOCK_LOTS} rm_sl ON rm_sl."Id" = ci."StockLotId"
        LEFT JOIN {DbTables.RAW_MATERIALS} rm ON rm."Id" = ci."RawMaterialId"
        WHERE ci."ProductionBatchId" = @BatchId;
        """;
}