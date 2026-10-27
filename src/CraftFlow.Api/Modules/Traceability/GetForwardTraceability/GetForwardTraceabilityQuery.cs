using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.Traceability;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Traceability.GetForwardTraceability;

public sealed record GetForwardTraceabilityQuery(Guid StockLotId)
    : IRequest<Result<ForwardTraceabilityDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_VIEW;
}