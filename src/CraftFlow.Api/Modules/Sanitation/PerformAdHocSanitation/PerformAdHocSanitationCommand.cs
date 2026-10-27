using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation.PerformAdHocSanitation;

public record PerformAdHocSanitationCommand(
    SanitationTargetType TargetType,
    Guid TargetId,
    string ChemicalsUsed,
    string? Notes = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SANITATION_EXECUTE;
}