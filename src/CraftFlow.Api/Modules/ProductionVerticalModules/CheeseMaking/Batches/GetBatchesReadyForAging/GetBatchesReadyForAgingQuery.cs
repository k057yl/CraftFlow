using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Production.Batches.GetBatchesReadyForAging;

public record GetBatchesReadyForAgingQuery : IRequest<Result<List<BatchReadyForAgingDto>>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_VIEW;
}