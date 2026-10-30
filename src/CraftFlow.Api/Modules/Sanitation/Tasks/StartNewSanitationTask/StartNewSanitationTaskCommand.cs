using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.StartNewSanitationTask;

public record StartNewSanitationTaskCommand(
    SanitationTargetType TargetType,
    Guid TargetId,
    Guid? PerformerUserId = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_EXECUTE;
}