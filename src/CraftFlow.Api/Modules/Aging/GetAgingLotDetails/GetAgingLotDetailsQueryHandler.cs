using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Common.MultiTenancy;
using Dapper;
using System.Data;

namespace CraftFlow.Api.Modules.Aging.GetAgingLotDetails;

public class GetAgingLotDetailsQueryHandler
{
    private readonly IDbConnection _dbConnection;
    private readonly ITenantContext _tenantContext;

    public GetAgingLotDetailsQueryHandler(IDbConnection dbConnection, ITenantContext tenantContext)
    {
        _dbConnection = dbConnection;
        _tenantContext = tenantContext;
    }

    public async Task<GetAgingLotDetailsDto?> HandleAsync(Guid lotId)
    {
        const string sql = $"""
            SELECT 
                al."Id" AS "LotId",
                al."ProductId" AS "ProductId",
                p."Name" AS "ProductName",
                uom."Code" AS "UnitName",
                COALESCE((
                    SELECT 
                        SUM(ci."Quantity" * sl."UnitPrice") * (1.0 + COALESCE(MAX(pb."OverheadPercentage"), 0.0) / 100.0)
                    FROM public.{DbTables.CONSUMED_INGREDIENTS} ci
                    JOIN public.{DbTables.STOCK_LOTS} sl ON sl."Id" = ci."StockLotId"
                    LEFT JOIN public.{DbTables.PRODUCTION_BATCHES} pb ON pb."Id" = ci."ProductionBatchId"
                    WHERE ci."ProductionBatchId" = al."ProductionBatchId"
                ), 0.0)::numeric AS "TotalBatchCost",
                COALESCE(SUM(CASE WHEN ali."State" = 1 THEN ali."CurrentWeight" ELSE 0 END), al."InitialQuantity") AS "InitialQuantity",
                COALESCE(SUM(CASE WHEN ali."State" = 1 THEN 1 ELSE 0 END), 0)::integer AS "UnitsCount"
            FROM {DbSchemas.AGING}.{DbTables.AGING_LOTS} al
            JOIN public.{DbTables.PRODUCTS} p ON p."Id" = al."ProductId"
            JOIN public.{DbTables.UNITS_OF_MEASURE} uom ON uom."Id" = p."UnitOfMeasureId"
            LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_LOT_ITEMS} ali ON ali."AgingLotId" = al."Id"
            WHERE al."Id" = @LotId AND al."TenantId" = @TenantId
            GROUP BY al."Id", p."Name", uom."Code";
            """;

        return await _dbConnection.QueryFirstOrDefaultAsync<GetAgingLotDetailsDto>(
            sql,
            new { LotId = lotId, TenantId = _tenantContext.TenantId }
        );
    }
}