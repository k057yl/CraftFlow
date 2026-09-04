using CraftFlow.Api.Common.Constants;

namespace CraftFlow.Api.Modules.Traceability.TraceabilityRead;

public static class TraceabilityConstants
{
    public const string SQL_GET_FORWARD_HEADER = $"""
        SELECT 
            sl."Id" AS RawMaterialStockLotId,
            COALESCE(sl."BatchNumber", 'Б/Н') AS RawMaterialBatchNumber,
            COALESCE(rm."Name", @DefaultMaterialName) AS RawMaterialName
        FROM {DbTables.STOCK_LOTS} sl
        INNER JOIN {DbTables.RAW_MATERIALS} rm ON rm."Id" = sl."ItemId"
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
            al."State"::text AS aging_status
        FROM {DbTables.CONSUMED_INGREDIENTS} ci
        JOIN {DbTables.PRODUCTION_BATCHES} pb ON pb."Id" = ci."ProductionBatchId"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_LOTS} al ON al."ProductionBatchId" = pb."Id"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_CHAMBERS} ach ON ach."Id" = al."AgingChamberId"
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
            pb."ActualOutputQuantity" AS brew_output_quantity,
            COALESCE(EXTRACT(DAY FROM (COALESCE(al."ActualReleaseDate", NOW()) - al."PlacedAt"))::integer, 0) AS aging_days,
            ach."Name" AS chamber_name
        FROM {DbTables.STOCK_LOTS} sl
        INNER JOIN {DbTables.PRODUCTS} p ON p."Id" = sl."ItemId"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_LOTS} al ON al."ProductId" = p."Id" OR al."ProductionBatchId" = sl."ProductionBatchId"
        LEFT JOIN {DbTables.PRODUCTION_BATCHES} pb ON pb."Id" = COALESCE(sl."ProductionBatchId", al."ProductionBatchId")
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_CHAMBERS} ach ON ach."Id" = al."AgingChamberId"
        WHERE sl."Id" = @ProductStockLotId AND sl."TenantId" = @TenantId
        ORDER BY al."PlacedAt" DESC NULLS LAST
        LIMIT 1;
        """;

    public const string SQL_GET_BACKWARD_CONSUMED = $"""
        SELECT 
            COALESCE(rm_sl."Id", ci."StockLotId") AS raw_material_stock_lot_id,
            COALESCE(rm."Name", 'Сырье') AS raw_material_name,
            COALESCE(rm_sl."BatchNumber", 'Б/Н') AS batch_number,
            ci."Quantity" AS quantity_used,
            COALESCE(uom."Code", '') AS unit_of_measure,
            '—' AS supplier_name
        FROM {DbTables.CONSUMED_INGREDIENTS} ci
        LEFT JOIN {DbTables.STOCK_LOTS} rm_sl ON rm_sl."Id" = ci."StockLotId"
        LEFT JOIN {DbTables.RAW_MATERIALS} rm ON rm."Id" = ci."RawMaterialId"
        LEFT JOIN {DbTables.UNITS_OF_MEASURE} uom ON uom."Id" = rm."UnitOfMeasureId"
        WHERE ci."ProductionBatchId" = @BatchId;
        """;
}