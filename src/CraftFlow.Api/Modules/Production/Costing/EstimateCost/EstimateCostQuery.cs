using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Costing.EstimateCost;

public record EstimateCostQuery(
    Guid RecipeId,
    decimal PlannedQty
) : IRequest<Result<decimal>>;