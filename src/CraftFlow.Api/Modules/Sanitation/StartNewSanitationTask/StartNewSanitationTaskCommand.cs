using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.StartNewSanitationTask;

public record StartNewSanitationTaskCommand(
    SanitationTargetType TargetType,
    Guid TargetId
) : IRequest<Result<Guid>>;