using CraftFlow.Api.Core.Authorization;
using CraftFlow.Api.Core.Behaviors;
using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.MRP.CalculateRequirements;

public sealed record CalculateRequirementsQuery(List<Guid>? BatchIds = null)
    : IRequest<Result<MrpReportDto>>, IRequirePermission, IRequirePlanFeature
{
    public AppPermission RequiredPermission => AppPermission.PROCUREMENT_VIEW;
    public PlanFeatures RequiredFeature => PlanFeatures.MRP_PLANNING;
}