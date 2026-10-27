using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Costing.EstimateCost;

public record EstimateCostQuery(
    Guid RecipeId,
    decimal PlannedQty
) : IRequest<Result<decimal>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_VIEW;
}