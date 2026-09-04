using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Common.MultiTenancy;
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
        const string sql = $"""
            SELECT 
                pb."Id" AS Id,
                pb."Name" AS BatchName,
                r."Name" AS RecipeName,
                pb."PlannedOutputQuantity" AS PlannedOutputQuantity,
                pb."StartedAt" AS StartedAt,
                pb."TargetDurationMinutes" AS TargetDurationMinutes,
                GREATEST(0, EXTRACT(EPOCH FROM (NOW() - pb."StartedAt")) / 60)::INT AS ElapsedMinutes,
                (EXTRACT(EPOCH FROM (NOW() - pb."StartedAt")) / 60) >= pb."TargetDurationMinutes" AS IsOverdue,
                pb."State"::TEXT AS State
            FROM production_batches pb
            JOIN recipes r ON r."Id" = pb."RecipeId"
            WHERE pb."State" = 2
              AND pb."TenantId" = @TenantId
            ORDER BY pb."StartedAt" ASC;
            """;

        var summary = await _dbConnection.QueryAsync<ActiveBatchSummaryDto>(
            sql,
            new { TenantId = _tenantContext.TenantId }
        );

        return Result.Success(summary);
    }
}