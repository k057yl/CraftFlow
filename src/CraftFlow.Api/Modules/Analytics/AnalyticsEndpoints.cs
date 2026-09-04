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

        group.MapGet(Endpoints.DASHBOARD, async (ISender sender) =>
        {
            var result = await sender.Send(new GetDashboardSummaryQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.AUDIT_LOGS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetAuditLogsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}