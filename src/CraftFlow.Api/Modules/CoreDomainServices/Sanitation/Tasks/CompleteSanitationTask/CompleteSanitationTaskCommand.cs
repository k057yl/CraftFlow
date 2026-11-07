using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.CompleteSanitationTask;

public record CompleteSanitationTaskCommand(
    Guid TaskId,
    string ChemicalsUsed,
    string? Notes = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_EXECUTE;
}