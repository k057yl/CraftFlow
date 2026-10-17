using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.CancelSanitationTask;

public record CancelSanitationTaskCommand(
    Guid TaskId,
    string Reason
) : IRequest<Result>;