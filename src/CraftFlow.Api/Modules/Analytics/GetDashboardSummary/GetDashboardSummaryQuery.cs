using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Analytics.GetDashboardSummary;

public record GetDashboardSummaryQuery : IRequest<Result<DashboardSummaryDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.DASHBOARD_VIEW;
}