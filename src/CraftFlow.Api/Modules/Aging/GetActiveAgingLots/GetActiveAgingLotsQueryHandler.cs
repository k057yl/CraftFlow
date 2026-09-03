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
                al."Id" AS LotId,
                al."BatchNumber" AS BatchNumber,
                ach."Name" AS ChamberName,
                al."UnitsCount" AS UnitsCount,
                al."CurrentQuantity" AS InitialQuantity,
                EXTRACT(DAY FROM (NOW() - al."PlacedAt"))::integer AS DaysInChamber,
                EXTRACT(DAY FROM (al."TargetReleaseDate" - al."PlacedAt"))::integer AS TargetDays,
                (NOW() >= al."TargetReleaseDate") AS IsReadyForRelease
            FROM {DbSchemas.AGING}.{DbTables.AGING_LOTS} al
            JOIN {DbSchemas.AGING}.{DbTables.AGING_CHAMBERS} ach ON ach."Id" = al."AgingChamberId"
            WHERE al."State" = 1 AND al."TenantId" = @TenantId
            ORDER BY al."PlacedAt" ASC;
            """;

        var result = await _dbConnection.QueryAsync<AgingLotSummaryDto>(sql, new { TenantId = _tenantContext.TenantId });
        return result.ToList();
    }
}