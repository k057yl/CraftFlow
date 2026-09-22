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
                al."UnitsCount" AS "UnitsCount",
                al."CurrentQuantity" AS "InitialQuantity",
                EXTRACT(DAY FROM (NOW() - al."PlacedAt"))::integer AS "DaysInChamber",
                EXTRACT(DAY FROM (al."TargetReleaseDate" - al."PlacedAt"))::integer AS "TargetDays",
                (NOW() >= al."TargetReleaseDate") AS "IsReadyForRelease"
            FROM aging.aging_lots al
            JOIN aging.aging_chambers ach ON ach."Id" = al."AgingChamberId"
            WHERE al."TenantId" = @TenantId AND (al."State" = 1 OR al."State" = 2)
            ORDER BY al."PlacedAt" ASC;
            """;

        var result = await _dbConnection.QueryAsync<AgingLotSummaryDto>(sql, new { TenantId = _tenantContext.TenantId });
        return result.ToList();
    }
}