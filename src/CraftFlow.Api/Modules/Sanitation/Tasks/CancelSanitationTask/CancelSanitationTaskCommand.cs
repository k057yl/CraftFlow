using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.CancelSanitationTask;

public record CancelSanitationTaskCommand(
    Guid TaskId,
    string Reason
) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_SCHEDULE;
}