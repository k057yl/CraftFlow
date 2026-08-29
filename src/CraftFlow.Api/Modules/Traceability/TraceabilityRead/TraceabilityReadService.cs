using CraftFlow.Api.Modules.Traceability.Contracts;
using Dapper;
using System.Data;

namespace CraftFlow.Api.Modules.Traceability.TraceabilityRead;

public sealed class TraceabilityReadService
{
    private readonly IDbConnection _dbConnection;

    public TraceabilityReadService(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<ForwardTraceabilityDto?> GetForwardTraceabilityAsync(Guid stockLotId, Guid tenantId)
    {
        const string sql = """
            SELECT 
                sl."Id" AS RawMaterialStockLotId,
                sl."BatchNumber" AS RawMaterialBatchNumber,
                COALESCE(rm."Name", p."Name", 'N/A') AS RawMaterialName
            FROM stock_lots sl
            LEFT JOIN "RawMaterials" rm ON rm."Id" = sl."ItemId"
            LEFT JOIN products p ON p."Id" = sl."ItemId"
            WHERE sl."Id" = @StockLotId AND sl."TenantId" = @TenantId;

            SELECT 
                pb."Id" AS ProductionBatchId,
                pb."Status"::text AS BatchStatus,
                pb."StartedAt",
                pb."CompletedAt",
                al."Id" AS AgingLotId,
                al."BatchNumber" AS AgingBatchNumber,
                ach."Name" AS ChamberName,
                al."Status"::text AS AgingStatus
            FROM production_batches pb
            LEFT JOIN aging."AgingLots" al ON al."ProductionBatchId" = pb."Id"
            LEFT JOIN aging."AgingChambers" ach ON ach."Id" = al."AgingChamberId"
            WHERE pb."WarehouseId" = (SELECT "WarehouseId" FROM stock_lots WHERE "Id" = @StockLotId) 
              AND pb."TenantId" = @TenantId;
            """;

        using var multi = await _dbConnection.QueryMultipleAsync(sql, new { StockLotId = stockLotId, TenantId = tenantId });
        var header = await multi.ReadFirstOrDefaultAsync<ForwardTraceabilityHeader>();

        if (header is null) return null;

        var batchRows = (await multi.ReadAsync<dynamic>())?.ToList() ?? new List<dynamic>();

        var batches = batchRows
            .GroupBy(r => (Guid)r.productionbatchid)
            .Select(g => new TraceabilityProductionBatchDto(
                g.Key,
                (string)g.First().batchstatus,
                (DateTime?)g.First().startedat ?? DateTime.MinValue,
                (DateTime?)g.First().completedat,
                g.Where(r => r.aginglotid != null)
                 .Select(r => new TraceabilityAgingLotDto(
                     (Guid)r.aginglotid,
                     (string)r.agingbatchnumber,
                     (string)(r.chambername ?? "Камера"),
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
        const string sql = """
            SELECT 
                so."Id" AS SalesOrderId,
                c."Name" AS CustomerName,
                sl."Id" AS ProductStockLotId,
                sl."BatchNumber" AS ProductBatchNumber,
                p."Name" AS ProductName,
                pb."Id" AS ProductionBatchId,
                pb."Status"::text AS BatchStatus,
                pb."StartedAt",
                pb."CompletedAt"
            FROM stock_lots sl
            JOIN products p ON p."Id" = sl."ItemId"
            LEFT JOIN "SalesOrderItem" soi ON soi."ProductId" = p."Id"
            LEFT JOIN sales_orders so ON so."Id" = soi."SalesOrderId"
            LEFT JOIN customers c ON c."Id" = so."CustomerId"
            LEFT JOIN production_batches pb ON pb."TargetProductId" = p."Id"
            WHERE sl."Id" = @ProductStockLotId AND sl."TenantId" = @TenantId;

            SELECT 
                ri."RecipeId",
                rmSl."Id" AS RawMaterialStockLotId,
                rm."Name" AS RawMaterialName,
                rmSl."BatchNumber",
                ri."Quantity" AS QuantityUsed
            FROM "RecipeIngredients" ri
            JOIN stock_lots rmSl ON rmSl."ItemId" = ri."RawMaterialId"
            JOIN "RawMaterials" rm ON rm."Id" = ri."RawMaterialId"
            WHERE rmSl."TenantId" = @TenantId;
            """;

        using var multi = await _dbConnection.QueryMultipleAsync(sql, new { ProductStockLotId = productStockLotId, TenantId = tenantId });
        var header = await multi.ReadFirstOrDefaultAsync<dynamic>();

        if (header is null || header.productionbatchid == null) return null;

        var rawIngredients = await multi.ReadAsync<dynamic>();
        var ingredients = rawIngredients?
            .Select(i => new TraceabilityIngredientDto(
                (Guid)i.rawmaterialstocklotid,
                (string)i.rawmaterialname,
                (string)i.batchnumber,
                (decimal)i.quantityused
            )).ToList() ?? new List<TraceabilityIngredientDto>();

        var originBatch = new TraceabilityProductionBatchDto(
            (Guid)header.productionbatchid,
            (string)header.batchstatus,
            (DateTime?)header.startedat ?? DateTime.MinValue,
            (DateTime?)header.completedat,
            new List<TraceabilityAgingLotDto>(),
            ingredients
        );

        return new BackwardTraceabilityDto(
            header.salesorderid != null ? (Guid)header.salesorderid : Guid.Empty,
            header.customername ?? "Без покупателя",
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