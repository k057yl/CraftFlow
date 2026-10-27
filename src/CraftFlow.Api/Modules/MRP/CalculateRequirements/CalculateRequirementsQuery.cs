using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.MRP.CalculateRequirements;

public sealed record CalculateRequirementsQuery : IRequest<Result<MrpReportDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PROCUREMENT_VIEW;
}