using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.CancelSanitationTask;

public record CancelSanitationTaskCommand(
    Guid TaskId,
    string Reason
) : IRequest<Result>;