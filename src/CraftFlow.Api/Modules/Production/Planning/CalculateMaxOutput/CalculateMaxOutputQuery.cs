using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Planning.CalculateMaxOutput;

public record CalculateMaxOutputQuery(
    Guid RecipeId,
    Guid WarehouseId
) : IRequest<Result<decimal>>;