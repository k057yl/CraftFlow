using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Sanitation;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.GetSanitationDetails;

public sealed record GetSanitationDetailsQuery(
    SanitationTargetType TargetType,
    Guid TargetId
) : IRequest<Result<SanitationDetailsDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_VIEW;
}