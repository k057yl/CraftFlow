using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Analytics.GetDashboardSummary
{
    public record GetDashboardSummaryQuery : IRequest<Result<DashboardSummaryDto>>;
}
