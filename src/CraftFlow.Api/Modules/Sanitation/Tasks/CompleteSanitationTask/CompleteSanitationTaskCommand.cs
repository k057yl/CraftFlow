using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.CompleteSanitationTask;

public record CompleteSanitationTaskCommand(
    Guid TaskId,
    string ChemicalsUsed,
    string? Notes = null
) : IRequest<Result<Guid>>;