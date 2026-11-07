using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Production.Batches.GetActiveBatchesSummary;

public record GetActiveBatchesSummaryQuery : IRequest<Result<IEnumerable<ActiveBatchSummaryDto>>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_VIEW;
}