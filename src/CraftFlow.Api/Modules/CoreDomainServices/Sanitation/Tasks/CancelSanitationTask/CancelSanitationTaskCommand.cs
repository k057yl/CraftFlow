using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.CancelSanitationTask;

public record CancelSanitationTaskCommand(
    Guid TaskId,
    string Reason
) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_SCHEDULE;
}