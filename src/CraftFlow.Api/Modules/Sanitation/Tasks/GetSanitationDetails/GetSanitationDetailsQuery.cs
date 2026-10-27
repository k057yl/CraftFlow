using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.Sanitation;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.GetSanitationDetails;

public sealed record GetSanitationDetailsQuery(
    SanitationTargetType TargetType,
    Guid TargetId
) : IRequest<Result<SanitationDetailsDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_VIEW;
}