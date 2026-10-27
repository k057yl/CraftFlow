using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.StartSanitationTask;

public record StartSanitationTaskCommand(
    Guid TaskId,
    Guid PerformerUserId
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_EXECUTE;
}