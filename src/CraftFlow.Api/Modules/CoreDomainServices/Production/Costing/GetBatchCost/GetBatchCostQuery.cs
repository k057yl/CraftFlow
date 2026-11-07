using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Production.Costing.GetBatchCost;

public record GetBatchCostQuery(Guid BatchId) : IRequest<Result<BatchCostDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_VIEW;
}
