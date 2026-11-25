using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Costing.ConsumeIngredient;

public sealed record ConsumeIngredientCommand(
    Guid CheeseBatchId,
    Guid StockLotId,
    Guid RawMaterialId,
    decimal Quantity
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}