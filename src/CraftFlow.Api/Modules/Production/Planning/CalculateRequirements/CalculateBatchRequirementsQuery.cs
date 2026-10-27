using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Planning.CalculateRequirements;

public record CalculateBatchRequirementsQuery(
    Guid RecipeId,
    Guid WarehouseId,
    decimal PlannedQty
) : IRequest<Result<List<RequirementItemDto>>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_VIEW;
}