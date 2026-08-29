using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Modules.Traceability.Contracts;
using CraftFlow.Api.Modules.Traceability.TraceabilityRead;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Traceability.GetBackwardTraceability;
public sealed class GetBackwardTraceabilityQueryHandler : IRequestHandler<GetBackwardTraceabilityQuery, Result<BackwardTraceabilityDto>>
{
    private readonly TraceabilityReadService _readService;
    private readonly ITenantContext _tenantContext;

    public GetBackwardTraceabilityQueryHandler(TraceabilityReadService readService, ITenantContext tenantContext)
    {
        _readService = readService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<BackwardTraceabilityDto>> Handle(GetBackwardTraceabilityQuery request, CancellationToken cancellationToken)
    {
        var result = await _readService.GetBackwardTraceabilityAsync(request.ProductStockLotId, _tenantContext.TenantId);

        if (result is null)
        {
            return Result.Failure<BackwardTraceabilityDto>(Error.NotFound(ErrorCodes.Inventory.ITEM_NOT_FOUND));
        }

        return Result.Success(result);
    }
}