using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.Api.Modules.Traceability.Contracts;
using CraftFlow.SharedKernel.Constants;
using Dapper;
using System.Data;

namespace CraftFlow.Api.Modules.Traceability.TraceabilityRead;

public static class TraceabilityConstants
{
    public const string SQL_GET_FORWARD_HEADER = """
        SELECT 
            sl."Id" AS RawMaterialStockLotId,
            sl."BatchNumber" AS RawMaterialBatchNumber,
            COALESCE(rm."Name", p."Name", @DefaultMaterialName) AS RawMaterialName
        FROM stock_lots sl
        LEFT JOIN "RawMaterials" rm ON rm."Id" = sl."ItemId"
        LEFT JOIN products p ON p."Id" = sl."ItemId"
        WHERE sl."Id" = @StockLotId AND sl."TenantId" = @TenantId;
        """;

    public const string SQL_GET_FORWARD_BATCHES = """
        /* 
           CRITICAL UPDATE:
           Querying production batches through actual consumed ingredients junction 
           instead of matching entire warehouse contents.
        */
        SELECT 
            pb."Id" AS ProductionBatchId,
            pb."Status" AS BatchStatusInt,
            pb."StartedAt",
            pb."CompletedAt",
            al."Id" AS AgingLotId,
            al."BatchNumber" AS AgingBatchNumber,
            ach."Name" AS ChamberName,
            al."Status"::text AS AgingStatus
        FROM "ConsumedIngredients" ci
        JOIN production_batches pb ON pb."Id" = ci."ProductionBatchId"
        LEFT JOIN aging."AgingLots" al ON al."ProductionBatchId" = pb."Id"
        LEFT JOIN aging."AgingChambers" ach ON ach."Id" = al."AgingChamberId"
        WHERE ci."StockLotId" = @StockLotId AND pb."TenantId" = @TenantId;
        """;

    public const string SQL_GET_BACKWARD_HEADER = """
        SELECT 
            so."Id" AS SalesOrderId,
            c."Name" AS CustomerName,
            sl."Id" AS ProductStockLotId,
            sl."BatchNumber" AS ProductBatchNumber,
            p."Name" AS ProductName,
            pb."Id" AS ProductionBatchId,
            pb."Status" AS BatchStatusInt,
            pb."StartedAt",
            pb."CompletedAt"
        FROM stock_lots sl
        JOIN products p ON p."Id" = sl."ItemId"
        LEFT JOIN "SalesOrderItem" soi ON soi."ProductId" = p."Id"
        LEFT JOIN sales_orders so ON so."Id" = soi."SalesOrderId"
        LEFT JOIN customers c ON c."Id" = so."CustomerId"
        LEFT JOIN production_batches pb ON pb."Id" = sl."ProductionBatchId"
        WHERE sl."Id" = @ProductStockLotId AND sl."TenantId" = @TenantId;
        """;

    public const string SQL_GET_BACKWARD_CONSUMED = """
        SELECT 
            rmSl."Id" AS RawMaterialStockLotId,
            rm."Name" AS RawMaterialName,
            rmSl."BatchNumber",
            ci."Quantity" AS QuantityUsed
        FROM "ConsumedIngredients" ci
        JOIN stock_lots rmSl ON rmSl."Id" = ci."StockLotId"
        JOIN "RawMaterials" rm ON rm."Id" = ci."RawMaterialId"
        WHERE ci."ProductionBatchId" = @BatchId AND rmSl."TenantId" = @TenantId;
        """;
}

public sealed class TraceabilityReadService
{
    private readonly IDbConnection _dbConnection;

    public TraceabilityReadService(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<ForwardTraceabilityDto?> GetForwardTraceabilityAsync(Guid stockLotId, Guid tenantId)
    {
        var header = await _dbConnection.QueryFirstOrDefaultAsync<ForwardTraceabilityHeader>(
            TraceabilityConstants.SQL_GET_FORWARD_HEADER,
            new
            {
                StockLotId = stockLotId,
                TenantId = tenantId,
                DefaultMaterialName = FormattingConstants.CONST_DEFAULT_MATERIAL_NAME
            });

        if (header is null) return null;

        var batchRows = (await _dbConnection.QueryAsync<dynamic>(
            TraceabilityConstants.SQL_GET_FORWARD_BATCHES,
            new { StockLotId = stockLotId, TenantId = tenantId }))?.ToList() ?? new List<dynamic>();

        var batches = batchRows
            .GroupBy(r => (Guid)r.productionbatchid)
            .Select(g => new TraceabilityProductionBatchDto(
                g.Key,
                ((BatchStatus)(int)g.First().batchstatusint).ToString(),
                (DateTime?)g.First().startedat ?? DateTime.MinValue,
                (DateTime?)g.First().completedat,
                g.Where(r => r.aginglotid != null)
                 .Select(r => new TraceabilityAgingLotDto(
                     (Guid)r.aginglotid,
                     (string)r.agingbatchnumber,
                     (string)(r.chambername ?? FormattingConstants.CONST_DEFAULT_CHAMBER_NAME),
                     (string)r.agingstatus
                 )).ToList() ?? new List<TraceabilityAgingLotDto>(),
                new List<TraceabilityIngredientDto>()
            )).ToList() ?? new List<TraceabilityProductionBatchDto>();

        return new ForwardTraceabilityDto(
            header.RawMaterialStockLotId,
            header.RawMaterialBatchNumber,
            header.RawMaterialName,
            batches
        );
    }

    public async Task<BackwardTraceabilityDto?> GetBackwardTraceabilityAsync(Guid productStockLotId, Guid tenantId)
    {
        var header = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
            TraceabilityConstants.SQL_GET_BACKWARD_HEADER,
            new { ProductStockLotId = productStockLotId, TenantId = tenantId });

        if (header is null || header.productionbatchid == null) return null;

        var rawIngredients = await _dbConnection.QueryAsync<dynamic>(
            TraceabilityConstants.SQL_GET_BACKWARD_CONSUMED,
            new { BatchId = (Guid)header.productionbatchid, TenantId = tenantId });

        var ingredients = rawIngredients?
            .Select(i => new TraceabilityIngredientDto(
                (Guid)i.rawmaterialstocklotid,
                (string)i.rawmaterialname,
                (string)i.batchnumber,
                (decimal)i.quantityused
            )).ToList() ?? new List<TraceabilityIngredientDto>();

        var originBatch = new TraceabilityProductionBatchDto(
            (Guid)header.productionbatchid,
            ((BatchStatus)(int)header.batchstatusint).ToString(),
            (DateTime?)header.startedat ?? DateTime.MinValue,
            (DateTime?)header.completedat,
            new List<TraceabilityAgingLotDto>(),
            ingredients
        );

        return new BackwardTraceabilityDto(
            header.salesorderid != null ? (Guid)header.salesorderid : Guid.Empty,
            header.customername ?? FormattingConstants.CONST_DEFAULT_CUSTOMER_NAME,
            (Guid)header.productstocklotid,
            (string)header.productbatchnumber,
            (string)header.productname,
            originBatch
        );
    }

    private record ForwardTraceabilityHeader(
        Guid RawMaterialStockLotId,
        string RawMaterialBatchNumber,
        string RawMaterialName
    );
}