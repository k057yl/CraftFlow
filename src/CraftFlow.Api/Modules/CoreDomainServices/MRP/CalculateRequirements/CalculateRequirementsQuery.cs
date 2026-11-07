using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.MRP.CalculateRequirements;

public sealed record CalculateRequirementsQuery(List<Guid>? BatchIds = null) : IRequest<Result<MrpReportDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PROCUREMENT_VIEW;
}