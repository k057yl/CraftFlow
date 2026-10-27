using CraftFlow.Api.Modules.Analytics.GetAuditLogs;
using CraftFlow.Api.Modules.Analytics.GetDashboardSummary;
using CraftFlow.SharedKernel.Constants;
using MediatR;

namespace CraftFlow.Api.Modules.Analytics;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Analytics")
            .RequireAuthorization();

        group.MapGet(AnalyticConstants.DASHBOARD, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetDashboardSummaryQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapGet(AnalyticConstants.AUDIT_LOGS, async ([AsParameters] GetAuditLogsQuery query, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(query, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });
    }
}