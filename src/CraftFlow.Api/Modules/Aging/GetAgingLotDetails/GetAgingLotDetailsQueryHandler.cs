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
                al."Id" AS LotId,
                al."ProductId" AS ProductId,
                p."Name" AS ProductName,
                uom."Code" AS UnitName,
                COALESCE((
                    SELECT SUM(ci."Quantity" * sl."UnitPrice")
                    FROM {DbTables.CONSUMED_INGREDIENTS} ci
                    JOIN {DbTables.STOCK_LOTS} sl ON sl."Id" = ci."StockLotId"
                    WHERE ci."ProductionBatchId" = al."ProductionBatchId"
                ), 0) AS TotalBatchCost,
                al."InitialQuantity" AS InitialQuantity,
                al."UnitsCount" AS UnitsCount
            FROM {DbSchemas.AGING}.{DbTables.AGING_LOTS} al
            JOIN {DbTables.PRODUCTS} p ON p."Id" = al."ProductId"
            JOIN {DbTables.UNITS_OF_MEASURE} uom ON uom."Id" = p."UnitOfMeasureId"
            WHERE al."Id" = @LotId AND al."TenantId" = @TenantId;
            """;

        return await _dbConnection.QueryFirstOrDefaultAsync<GetAgingLotDetailsDto>(
            sql,
            new { LotId = lotId, TenantId = _tenantContext.TenantId }
        );
    }
}