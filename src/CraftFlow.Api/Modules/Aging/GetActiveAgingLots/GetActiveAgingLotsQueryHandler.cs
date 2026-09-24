using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Common.MultiTenancy;
using Dapper;
using System.Data;

namespace CraftFlow.Api.Modules.Aging.GetActiveAgingLots;

public class GetActiveAgingLotsQueryHandler
{
    private readonly IDbConnection _dbConnection;
    private readonly ITenantContext _tenantContext;

    public GetActiveAgingLotsQueryHandler(IDbConnection dbConnection, ITenantContext tenantContext)
    {
        _dbConnection = dbConnection;
        _tenantContext = tenantContext;
    }

    public async Task<List<AgingLotSummaryDto>> HandleAsync()
    {
        const string sql = $"""
            SELECT 
                al."Id" AS "LotId",
                al."Id" AS "Id",
                al."BatchNumber" AS "BatchNumber",
                al."BatchNumber" AS "Name",
                ach."Name" AS "ChamberName",
                COALESCE(SUM(CASE WHEN ali."State" = 1 THEN 1 ELSE 0 END), 0)::integer AS "UnitsCount",
                COALESCE(SUM(CASE WHEN ali."State" = 1 THEN ali."CurrentWeight" ELSE 0 END), 0) AS "InitialQuantity",
                EXTRACT(DAY FROM (NOW() - al."PlacedAt"))::integer AS "DaysInChamber",
                EXTRACT(DAY FROM (al."TargetReleaseDate" - al."PlacedAt"))::integer AS "TargetDays",
                (NOW() >= al."TargetReleaseDate") AS "IsReadyForRelease"
            FROM {DbSchemas.AGING}.{DbTables.AGING_LOTS} al
            JOIN {DbSchemas.AGING}.{DbTables.AGING_CHAMBERS} ach ON ach."Id" = al."AgingChamberId"
            LEFT JOIN {DbSchemas.AGING}.{DbTables.AGING_LOT_ITEMS} ali ON ali."AgingLotId" = al."Id"
            WHERE al."TenantId" = @TenantId AND (al."State" = 1 OR al."State" = 2)
            GROUP BY al."Id", ach."Name"
            ORDER BY al."PlacedAt" ASC;
            """;

        var result = await _dbConnection.QueryAsync<AgingLotSummaryDto>(sql, new { TenantId = _tenantContext.TenantId });
        return result.ToList();
    }
}