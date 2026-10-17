using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.StartSanitationTask;

public record StartSanitationTaskCommand(
    Guid TaskId,
    Guid PerformerUserId
) : IRequest<Result<Guid>>;