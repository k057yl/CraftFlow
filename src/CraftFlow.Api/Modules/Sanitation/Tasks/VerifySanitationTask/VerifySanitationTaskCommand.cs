using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.VerifySanitationTask;

public record VerifySanitationTaskCommand(
    Guid TaskId,
    Guid VerifierUserId,
    bool IsPassed,
    string? VerificationNotes = null
) : IRequest<Result<Guid>>;