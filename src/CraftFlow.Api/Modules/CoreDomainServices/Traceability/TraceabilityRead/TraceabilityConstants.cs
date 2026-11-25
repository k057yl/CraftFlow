using CraftFlow.Api.Core.Constants;
using CraftFlow.SharedKernel.Constants;

namespace CraftFlow.Api.Modules.CoreDomainServices.Traceability.TraceabilityRead;

public static class TraceabilityConstants
{
    public const string SQL_GET_TRACEABILITY_RAW_LOTS = $"""
        SELECT 
            sl."Id" AS Id,
            COALESCE(rm."Name", '{FormattingConstants.CONST_DEFAULT_MATERIAL_NAME}') || 
            ' — ' || COALESCE(sl."BatchNumber", '{FormattingConstants.CONST_DEFAULT_BATCH_NUMBER}') || 
            ' (' || TO_CHAR(sl."CreatedDate", 'DD.MM.YYYY') || ', ' || ROUND(sl."Quantity", 2)::text || ' ' || COALESCE(uom."Code", '') || ')' AS Name
        FROM {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS} sl
        JOIN {DbSchemas.CATALOG}.{DbTables.RAW_MATERIALS} rm ON rm."Id" = sl."ItemId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.UNITS_OF_MEASURE} uom ON uom."Id" = sl."UnitOfMeasureId"
        WHERE sl."TenantId" = @TenantId
        ORDER BY sl."CreatedDate" DESC;
        """;

    public const string SQL_GET_TRACEABILITY_PRODUCT_LOTS = $"""
        SELECT 
            sl."Id" AS Id,
            COALESCE(p."Name", '{FormattingConstants.CONST_DEFAULT_MATERIAL_NAME}') || 
            ' — ' || COALESCE(sl."BatchNumber", '{FormattingConstants.CONST_DEFAULT_BATCH_NUMBER}') || 
            ' (' || TO_CHAR(sl."CreatedDate", 'DD.MM.YYYY') || ', ' || ROUND(sl."Quantity", 2)::text || ' ' || COALESCE(uom."Code", '') || ')' AS Name
        FROM {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS} sl
        JOIN {DbSchemas.CATALOG}.{DbTables.PRODUCTS} p ON p."Id" = sl."ItemId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.UNITS_OF_MEASURE} uom ON uom."Id" = sl."UnitOfMeasureId"
        WHERE sl."TenantId" = @TenantId
        ORDER BY sl."CreatedDate" DESC;
        """;

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
        FROM {DbSchemas.CHEESE_MAKING}.{DbTables.CHEESE_CONSUMED_INGREDIENTS} ci
        JOIN {DbSchemas.CHEESE_MAKING}.{DbTables.CHEESE_BATCHES} pb ON pb."Id" = ci."CheeseBatchId"
        WHERE (ci."StockLotId" = @StockLotId OR ci."RawMaterialId" = @StockLotId) 
          AND pb."TenantId" = @TenantId

        UNION ALL

        SELECT DISTINCT
            fb."Id" AS ProductionBatchId,
            CASE fb."Stage"
                WHEN 'COMPLETED' THEN 2
                WHEN 'DISCARDED' THEN 5
                ELSE 1
            END AS BatchStatusInt,
            fb."StartedAt" AS StartedAt,
            fb."CompletedAt" AS CompletedAt,
            COALESCE(fb."SubstrateWeightKg", 0) AS PlannedQuantity,
            COALESCE(fb."ActualOutputQuantity", 0) AS ActualQuantity
        FROM {DbSchemas.FERMENTATION}.{DbTables.FERMENTATION_CONSUMED_INGREDIENTS} fci
        JOIN {DbSchemas.FERMENTATION}.{DbTables.FERMENTATION_BATCHES} fb ON fb."Id" = fci."FermentationBatchId"
        WHERE (fci."StockLotId" = @StockLotId OR fci."RawMaterialId" = @StockLotId)
          AND fb."TenantId" = @TenantId;
        """;

    public const string SQL_GET_BACKWARD_HEADER = $"""
        WITH target_lot AS (
            SELECT "Id", "TenantId", "ItemId", "ProductionBatchId", "BatchNumber", "Quantity"
            FROM {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS}
            WHERE "Id" = @ProductStockLotId AND "TenantId" = @TenantId
        ),
        lot_items_total AS (
            SELECT "StockLotId", COALESCE(SUM("CurrentWeight"), 0) AS InitialReceivedWeight
            FROM {DbSchemas.INVENTORY}.{DbTables.STOCK_LOT_ITEMS}
            WHERE "StockLotId" = @ProductStockLotId
            GROUP BY "StockLotId"
        ),
        lot_sales AS (
            SELECT 
                soi."StockLotId",
                MIN(so."Id"::text)::uuid AS FirstSalesOrderId,
                MIN(c."Id"::text)::uuid AS FirstCustomerId,
                STRING_AGG(DISTINCT c."Name", ', ') AS AllCustomerNames
            FROM {DbSchemas.SALES}.{DbTables.SALES_ORDER_ITEMS} soi
            JOIN {DbSchemas.SALES}.{DbTables.SALES_ORDERS} so ON so."Id" = soi."SalesOrderId"
            JOIN {DbSchemas.SALES}.{DbTables.CUSTOMERS} c ON c."Id" = so."CustomerId"
            WHERE soi."StockLotId" = @ProductStockLotId
            GROUP BY soi."StockLotId"
        )
        SELECT 
            tl."Id" AS ProductStockLotId,
            COALESCE(tl."BatchNumber", pb."Name", fb."BatchNumber", '{FormattingConstants.CONST_DEFAULT_BATCH_NUMBER}') AS ProductBatchNumber,
            COALESCE(p."Name", '{FormattingConstants.CONST_DEFAULT_MATERIAL_NAME}') AS ProductName,
            tl."Quantity" AS CurrentStockQuantity,
            CASE 
                WHEN COALESCE(lit.InitialReceivedWeight, 0) > 0 THEN lit.InitialReceivedWeight
                WHEN tl."Quantity" > 0 THEN tl."Quantity"
                ELSE COALESCE(pb."ActualOutputQuantity", fb."ActualOutputQuantity", 0)
            END AS ReceivedQuantity,
            COALESCE(pb."Id", fb."Id") AS ProductionBatchId,
            COALESCE(
                pb."State",
                CASE fb."Stage"
                    WHEN 'COMPLETED' THEN 2
                    WHEN 'DISCARDED' THEN 5
                    ELSE 1
                END,
                0
            ) AS BatchStatusInt,
            COALESCE(pb."StartedAt", fb."StartedAt") AS StartedAt,
            COALESCE(pb."CompletedAt", fb."CompletedAt") AS CompletedAt,
            COALESCE(pb."PlannedOutputQuantity", fb."SubstrateWeightKg", 0) AS PlannedQuantity,
            COALESCE(pb."ActualOutputQuantity", fb."ActualOutputQuantity", 0) AS BrewOutputQuantity,
            COALESCE(EXTRACT(DAY FROM (COALESCE(al."ActualReleaseDate", NOW()) - al."PlacedAt"))::integer, 0) AS AgingDays,
            ach."Name" AS ChamberName,
            sloc."Name" AS LocationName,
            ls.FirstSalesOrderId AS SalesOrderId,
            ls.FirstCustomerId AS CustomerId,
            ls.AllCustomerNames AS CustomerName
        FROM target_lot tl
        LEFT JOIN lot_items_total lit ON lit."StockLotId" = tl."Id"
        LEFT JOIN lot_sales ls ON ls."StockLotId" = tl."Id"
        LEFT JOIN {DbSchemas.CHEESE_MAKING}.{DbTables.CHEESE_BATCHES} pb 
            ON pb."Id" = tl."ProductionBatchId"
        LEFT JOIN {DbSchemas.FERMENTATION}.{DbTables.FERMENTATION_BATCHES} fb
            ON fb."Id" = tl."ProductionBatchId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.PRODUCTS} p ON p."Id" = tl."ItemId"
        LEFT JOIN {DbSchemas.CHEESE_MAKING}.{DbTables.AGING_LOTS} al 
            ON al."ProductionBatchId" = pb."Id"
        LEFT JOIN {DbSchemas.CHEESE_MAKING}.{DbTables.AGING_CHAMBERS} ach ON ach."Id" = al."AgingChamberId"
        LEFT JOIN {DbSchemas.INVENTORY}.{DbTables.STORAGE_LOCATIONS} sloc ON sloc."Id" = al."StorageLocationId"
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
        FROM {DbSchemas.CHEESE_MAKING}.{DbTables.CHEESE_CONSUMED_INGREDIENTS} ci
        LEFT JOIN {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS} rm_sl ON rm_sl."Id" = ci."StockLotId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.RAW_MATERIALS} rm ON rm."Id" = ci."RawMaterialId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.UNITS_OF_MEASURE} uom ON uom."Id" = rm."UnitOfMeasureId"
        LEFT JOIN {DbSchemas.PROCUREMENT}.{DbTables.SUPPLIERS} sup ON sup."Id" = rm_sl."SupplierId"
        WHERE ci."CheeseBatchId" = @BatchId AND ci."TenantId" = @TenantId

        UNION ALL

        SELECT 
            COALESCE(rm_sl."Id", fci."StockLotId", fci."RawMaterialId") AS RawMaterialStockLotId,
            COALESCE(rm."Name", '{FormattingConstants.CONST_DEFAULT_MATERIAL_NAME}') AS RawMaterialName,
            COALESCE(rm_sl."BatchNumber", '{FormattingConstants.CONST_DEFAULT_BATCH_NUMBER}') AS BatchNumber,
            fci."Quantity" AS QuantityUsed,
            COALESCE(uom."Code", '') AS UnitOfMeasure,
            sup."Id" AS SupplierId,
            COALESCE(sup."Name", '{FormattingConstants.NOT_AVAILABLE}') AS SupplierName
        FROM {DbSchemas.FERMENTATION}.{DbTables.FERMENTATION_CONSUMED_INGREDIENTS} fci
        LEFT JOIN {DbSchemas.INVENTORY}.{DbTables.STOCK_LOTS} rm_sl ON rm_sl."Id" = fci."StockLotId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.RAW_MATERIALS} rm ON rm."Id" = fci."RawMaterialId"
        LEFT JOIN {DbSchemas.CATALOG}.{DbTables.UNITS_OF_MEASURE} uom ON uom."Id" = rm."UnitOfMeasureId"
        LEFT JOIN {DbSchemas.PROCUREMENT}.{DbTables.SUPPLIERS} sup ON sup."Id" = rm_sl."SupplierId"
        WHERE fci."FermentationBatchId" = @BatchId AND fci."TenantId" = @TenantId;
        """;
}