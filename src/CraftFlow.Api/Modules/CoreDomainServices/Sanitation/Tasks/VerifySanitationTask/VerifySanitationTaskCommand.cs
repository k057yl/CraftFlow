using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.VerifySanitationTask;

public record VerifySanitationTaskCommand(
    Guid TaskId,
    Guid VerifierUserId,
    bool IsPassed,
    string? VerificationNotes = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_VERIFY;
}