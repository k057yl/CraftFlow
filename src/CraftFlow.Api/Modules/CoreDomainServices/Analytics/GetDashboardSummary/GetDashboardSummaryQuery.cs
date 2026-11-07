using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Analytics;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Analytics.GetDashboardSummary;

public record GetDashboardSummaryQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<Result<DashboardSummaryDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.DASHBOARD_VIEW;
}