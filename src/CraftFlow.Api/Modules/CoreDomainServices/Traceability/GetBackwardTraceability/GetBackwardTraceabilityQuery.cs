using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Traceability;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Traceability.GetBackwardTraceability;

public sealed record GetBackwardTraceabilityQuery(Guid ProductStockLotId)
    : IRequest<Result<BackwardTraceabilityDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_VIEW;
}