using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Result;
using Dapper;
using MediatR;
using System.Data;

namespace CraftFlow.Api.Modules.Production.GetActiveBatchesSummary;

public sealed class GetActiveBatchesSummaryQueryHandler
    : IRequestHandler<GetActiveBatchesSummaryQuery, Result<IEnumerable<ActiveBatchSummaryDto>>>
{
    private readonly IDbConnection _dbConnection;
    private readonly ITenantContext _tenantContext;

    public GetActiveBatchesSummaryQueryHandler(IDbConnection dbConnection, ITenantContext tenantContext)
    {
        _dbConnection = dbConnection;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IEnumerable<ActiveBatchSummaryDto>>> Handle(
        GetActiveBatchesSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT 
                pb."Id" AS Id,
                pb."Name" AS BatchName,
                r."Name" AS RecipeName,
                pb."PlannedOutputQuantity" AS PlannedOutputQuantity,
                pb."StartedAt" AS StartedAt,
                pb."StartedAt" AS ScheduledAt,
                pb."TargetDurationMinutes" AS TargetDurationMinutes,
                CASE 
                    WHEN pb."State" = {(int)BatchState.InProgress} 
                        THEN GREATEST(0, EXTRACT(EPOCH FROM (NOW() - pb."StartedAt")) / 60)::INT 
                    ELSE 0 
                END AS ElapsedMinutes,
                CASE 
                    WHEN pb."State" = {(int)BatchState.InProgress} 
                        THEN (EXTRACT(EPOCH FROM (NOW() - pb."StartedAt")) / 60) >= pb."TargetDurationMinutes" 
                    ELSE FALSE 
                END AS IsOverdue,
                CASE pb."State"
                    WHEN {(int)BatchState.Draft} THEN '{BatchState.Draft}'
                    WHEN {(int)BatchState.InProgress} THEN '{BatchState.InProgress}'
                    ELSE pb."State"::TEXT
                END AS State
            FROM production.production_batches pb
            JOIN catalog.recipes r ON r."Id" = pb."RecipeId"
            WHERE pb."State" IN ({(int)BatchState.Draft}, {(int)BatchState.InProgress})
              AND pb."TenantId" = @TenantId
            ORDER BY pb."StartedAt" DESC;
            """;

        var summary = await _dbConnection.QueryAsync<ActiveBatchSummaryDto>(
            sql,
            new { TenantId = _tenantContext.TenantId }
        );

        return Result.Success(summary);
    }
}