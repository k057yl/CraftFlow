using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Costing.ConsumeIngredient;

public sealed record ConsumeIngredientCommand(
    Guid ProductionBatchId,
    Guid StockLotId,
    Guid RawMaterialId,
    decimal Quantity
) : IRequest<Result<Guid>>;