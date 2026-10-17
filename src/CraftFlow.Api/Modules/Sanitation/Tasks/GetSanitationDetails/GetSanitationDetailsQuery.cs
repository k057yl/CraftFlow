using CraftFlow.SharedKernel.Dtos.Sanitation;
using CraftFlow.SharedKernel.Enums.Sanitation;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.GetSanitationDetails;

public sealed record GetSanitationDetailsQuery(
    SanitationTargetType TargetType,
    Guid TargetId
) : IRequest<SanitationDetailsDto>;