using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
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
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_SCHEDULE;
}