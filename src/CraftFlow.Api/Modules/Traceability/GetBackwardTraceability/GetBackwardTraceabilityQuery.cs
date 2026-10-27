using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.Traceability;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Traceability.GetBackwardTraceability;

public sealed record GetBackwardTraceabilityQuery(Guid ProductStockLotId)
    : IRequest<Result<BackwardTraceabilityDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_VIEW;
}