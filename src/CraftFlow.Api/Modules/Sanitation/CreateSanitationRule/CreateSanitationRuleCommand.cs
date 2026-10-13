using CraftFlow.SharedKernel.Enums.Sanitation;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.CreateSanitationRule;

public sealed record CreateSanitationRuleCommand(
    SanitationTargetType TargetType,
    Guid TargetId,
    SanitationTriggerType TriggerType,
    string RequiredChemicals,
    int? IntervalDays = null,
    int? MaxBatchCount = null,
    DateTime? NextScheduledAt = null
) : IRequest<Guid>;