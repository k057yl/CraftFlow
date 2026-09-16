using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.CalculateRequirements;
public record CalculateBatchRequirementsQuery(
    Guid RecipeId,
    Guid WarehouseId,
    decimal PlannedQty
) : IRequest<Result<List<RequirementItemDto>>>;