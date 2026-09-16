using CraftFlow.Api.Modules.Analytics.GetAuditLogs;
using CraftFlow.Api.Modules.Analytics.GetDashboardSummary;
using MediatR;

namespace CraftFlow.Api.Modules.Analytics;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Analytics")
            .RequireAuthorization();

        group.MapGet(AnalyticConstants.DASHBOARD, async (ISender sender) =>
        {
            var result = await sender.Send(new GetDashboardSummaryQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(AnalyticConstants.AUDIT_LOGS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetAuditLogsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}