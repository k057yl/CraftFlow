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
                rm."Name" AS RawMaterialName
            FROM inventory."StockLots" sl
            JOIN catalog."RawMaterials" rm ON rm."Id" = sl."ItemId"
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
            FROM production."ProductionBatches" pb
            JOIN production."BatchIngredients" bi ON bi."ProductionBatchId" = pb."Id"
            LEFT JOIN aging."AgingLots" al ON al."ProductionBatchId" = pb."Id"
            LEFT JOIN aging."AgingChambers" ach ON ach."Id" = al."AgingChamberId"
            WHERE bi."StockLotId" = @StockLotId AND pb."TenantId" = @TenantId;
            """;

        using var multi = await _dbConnection.QueryMultipleAsync(sql, new { StockLotId = stockLotId, TenantId = tenantId });
        var header = await multi.ReadFirstOrDefaultAsync<ForwardTraceabilityDto>();

        if (header is null) return null;

        var batchRows = await multi.ReadAsync<dynamic>();

        var batches = batchRows
            .GroupBy(r => (Guid)r.productionbatchid)
            .Select(g => new TraceabilityProductionBatchDto(
                g.Key,
                (string)g.First().batchstatus,
                (DateTime)g.First().startedat,
                (DateTime?)g.First().completedat,
                g.Where(r => r.aginglotid != null)
                 .Select(r => new TraceabilityAgingLotDto(
                     (Guid)r.aginglotid,
                     (string)r.agingbatchnumber,
                     (string)r.chambername,
                     (string)r.agingstatus
                 )).ToList(),
                []
            )).ToList();

        return header with { Batches = batches };
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
            FROM inventory."StockLots" sl
            JOIN catalog."Products" p ON p."Id" = sl."ItemId"
            LEFT JOIN sales."SalesOrderItems" soi ON soi."ProductId" = p."Id"
            LEFT JOIN sales."SalesOrders" so ON so."Id" = soi."SalesOrderId"
            LEFT JOIN sales."Customers" c ON c."Id" = so."CustomerId"
            LEFT JOIN production."ProductionBatches" pb ON pb."TargetProductId" = p."Id"
            WHERE sl."Id" = @ProductStockLotId AND sl."TenantId" = @TenantId;

            SELECT 
                bi."ProductionBatchId",
                rmSl."Id" AS RawMaterialStockLotId,
                rm."Name" AS RawMaterialName,
                rmSl."BatchNumber",
                bi."QuantityUsed"
            FROM production."BatchIngredients" bi
            JOIN inventory."StockLots" rmSl ON rmSl."Id" = bi."StockLotId"
            JOIN catalog."RawMaterials" rm ON rm."Id" = rmSl."ItemId"
            WHERE bi."TenantId" = @TenantId;
            """;

        using var multi = await _dbConnection.QueryMultipleAsync(sql, new { ProductStockLotId = productStockLotId, TenantId = tenantId });
        var header = await multi.ReadFirstOrDefaultAsync<dynamic>();

        if (header is null) return null;

        var ingredients = (await multi.ReadAsync<dynamic>())
            .Where(i => i.productionbatchid == header.productionbatchid)
            .Select(i => new TraceabilityIngredientDto(
                (Guid)i.rawmaterialstocklotid,
                (string)i.rawmaterialname,
                (string)i.batchnumber,
                (decimal)i.quantityused
            )).ToList();

        var batchDto = new TraceabilityProductionBatchDto(
            (Guid)header.productionbatchid,
            (string)header.batchstatus,
            (DateTime)header.startedat,
            (DateTime?)header.completedat,
            [],
            ingredients
        );

        return new BackwardTraceabilityDto(
            header.salesorderid ?? Guid.Empty,
            header.customername ?? string.Empty,
            (Guid)header.productstocklotid,
            (string)header.productbatchnumber,
            (string)header.productname,
            batchDto
        );
    }
}