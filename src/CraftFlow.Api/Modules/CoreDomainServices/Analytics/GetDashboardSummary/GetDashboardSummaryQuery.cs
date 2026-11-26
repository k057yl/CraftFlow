using CraftFlow.Api.Core.Authorization;
using CraftFlow.Api.Core.Behaviors;
using CraftFlow.SharedKernel.Dtos.Analytics;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Analytics.GetDashboardSummary;

public record GetDashboardSummaryQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<Result<DashboardSummaryDto>>, IRequirePermission, IRequirePlanFeature
{
    public AppPermission RequiredPermission => AppPermission.DASHBOARD_VIEW;
    public PlanFeatures RequiredFeature => PlanFeatures.ADVANCED_ANALYTICS;
}