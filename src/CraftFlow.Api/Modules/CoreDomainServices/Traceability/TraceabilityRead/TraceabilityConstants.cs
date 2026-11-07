using CraftFlow.Api.Core.Constants;
using CraftFlow.SharedKernel.Constants;

namespace CraftFlow.Api.Modules.CoreDomainServices.Traceability.TraceabilityRead;

public static class TraceabilityConstants
{
    public const string SQL_GET_FORWARD_HEADER = $"""
        SELECT 
            sl."Id" AS RawMaterialStockLotId,
            COALESCE(sl."BatchNumber", '{FormattingConstants.CONST_DEFAULT_BATCH_NUMBER}') AS RawMaterialBatchNumber,
            COALESCE(rm."Name", @DefaultMaterialName) AS RawMaterialName
        FROM {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS} sl
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.RAW_MATERIALS} rm ON rm."Id" = sl."ItemId"
        WHERE sl."Id" = @StockLotId AND sl."TenantId" = @TenantId
        LIMIT 1;
        """;

    public const string SQL_GET_FORWARD_BATCHES = $"""
        SELECT DISTINCT
            pb."Id" AS ProductionBatchId,
            pb."State" AS BatchStatusInt,
            pb."StartedAt" AS StartedAt,
            pb."CompletedAt" AS CompletedAt,
            COALESCE(pb."PlannedOutputQuantity", 0) AS PlannedQuantity,
            COALESCE(pb."ActualOutputQuantity", 0) AS ActualQuantity
        FROM {DbSchemas.PRODUCTION}.{DbTables.CONSUMED_INGREDIENTS} ci
        JOIN {DbSchemas.PRODUCTION}.{DbTables.PRODUCTION_BATCHES} pb ON pb."Id" = ci."ProductionBatchId"
        WHERE (ci."StockLotId" = @StockLotId OR ci."RawMaterialId" = @StockLotId) 
          AND pb."TenantId" = @TenantId;
        """;

    public const string SQL_GET_BACKWARD_HEADER = $"""
        WITH target_lot AS (
            SELECT "Id", "TenantId", "ItemId", "ProductionBatchId", "BatchNumber", "Quantity"
            FROM {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS}
            WHERE "Id" = @ProductStockLotId AND "TenantId" = @TenantId
        )
        SELECT 
            tl."Id" AS ProductStockLotId,
            COALESCE(tl."BatchNumber", pb."Name", '{FormattingConstants.CONST_DEFAULT_BATCH_NUMBER}') AS ProductBatchNumber,
            COALESCE(p."Name", '{FormattingConstants.CONST_DEFAULT_MATERIAL_NAME}') AS ProductName,
            COALESCE(tl."Quantity", pb."ActualOutputQuantity", 0) AS ActualQuantity,
            pb."Id" AS ProductionBatchId,
            COALESCE(pb."State", 0) AS BatchStatusInt,
            pb."StartedAt" AS StartedAt,
            pb."CompletedAt" AS CompletedAt,
            COALESCE(pb."PlannedOutputQuantity", 0) AS PlannedQuantity,
            COALESCE(pb."ActualOutputQuantity", 0) AS BrewOutputQuantity,
            COALESCE(EXTRACT(DAY FROM (COALESCE(al."ActualReleaseDate", NOW()) - al."PlacedAt"))::integer, 0) AS AgingDays,
            ach."Name" AS ChamberName,
            sloc."Name" AS LocationName,
            so."Id" AS SalesOrderId,
            c."Id" AS CustomerId,
            c."Name" AS CustomerName
        FROM target_lot tl
        LEFT JOIN {DbSchemas.PRODUCTION}.{DbTables.PRODUCTION_BATCHES} pb 
            ON pb."Id" = tl."ProductionBatchId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.PRODUCTS} p ON p."Id" = tl."ItemId"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_LOTS} al 
            ON al."ProductionBatchId" = pb."Id"
        LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_CHAMBERS} ach ON ach."Id" = al."AgingChamberId"
        LEFT JOIN {DbSchemas.INVENTORY}.{DbTables.STORAGE_LOCATIONS} sloc ON sloc."Id" = al."StorageLocationId"
        LEFT JOIN {DbSchemas.SALES}.{DbTables.SALES_ORDER_ITEMS} soi ON soi."StockLotId" = tl."Id"
        LEFT JOIN {DbSchemas.SALES}.{DbTables.SALES_ORDERS} so ON so."Id" = soi."SalesOrderId"
        LEFT JOIN {DbSchemas.SALES}.{DbTables.CUSTOMERS} c ON c."Id" = so."CustomerId"
        ORDER BY al."PlacedAt" DESC NULLS LAST
        LIMIT 1;
        """;

    public const string SQL_GET_BACKWARD_CONSUMED = $"""
        SELECT 
            COALESCE(rm_sl."Id", ci."StockLotId", ci."RawMaterialId") AS RawMaterialStockLotId,
            COALESCE(rm."Name", '{FormattingConstants.CONST_DEFAULT_MATERIAL_NAME}') AS RawMaterialName,
            COALESCE(rm_sl."BatchNumber", '{FormattingConstants.CONST_DEFAULT_BATCH_NUMBER}') AS BatchNumber,
            ci."Quantity" AS QuantityUsed,
            COALESCE(uom."Code", '') AS UnitOfMeasure,
            sup."Id" AS SupplierId,
            COALESCE(sup."Name", '{FormattingConstants.NOT_AVAILABLE}') AS SupplierName
        FROM {DbSchemas.PRODUCTION}.{DbTables.CONSUMED_INGREDIENTS} ci
        LEFT JOIN {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS} rm_sl ON rm_sl."Id" = ci."StockLotId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.RAW_MATERIALS} rm ON rm."Id" = ci."RawMaterialId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.UNITS_OF_MEASURE} uom ON uom."Id" = rm."UnitOfMeasureId"
        LEFT JOIN {DbSchemas.PROCUREMENT}.{DbTables.SUPPLIERS} sup ON sup."Id" = rm_sl."SupplierId"
        WHERE ci."ProductionBatchId" = @BatchId AND ci."TenantId" = @TenantId;
        """;
}