using CraftFlow.Api.Core.Extensions;
using CraftFlow.Api.Modules.CoreDomainServices.Analytics.GetAuditLogs;
using CraftFlow.Api.Modules.CoreDomainServices.Analytics.GetDashboardSummary;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Analytics;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Analytics")
            .RequireAuthorization();

        group.MapGet(AnalyticConstants.DASHBOARD, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetDashboardSummaryQuery(), ct)).ToHttpResult());

        group.MapGet(AnalyticConstants.AUDIT_LOGS, async ([AsParameters] GetAuditLogsQuery query, ISender sender, CancellationToken ct) =>
            (await sender.Send(query, ct)).ToHttpResult());
    }
}