using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.SharedKernel.Dtos.Traceability;
using CraftFlow.Api.Modules.Traceability.TraceabilityRead;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Traceability.GetForwardTraceability;

public sealed class GetForwardTraceabilityQueryHandler : IRequestHandler<GetForwardTraceabilityQuery, Result<ForwardTraceabilityDto>>
{
    private readonly TraceabilityReadService _readService;
    private readonly ITenantContext _tenantContext;

    public GetForwardTraceabilityQueryHandler(TraceabilityReadService readService, ITenantContext tenantContext)
    {
        _readService = readService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<ForwardTraceabilityDto>> Handle(GetForwardTraceabilityQuery request, CancellationToken cancellationToken)
    {
        if (request.StockLotId == Guid.Empty)
        {
            return Result.Failure<ForwardTraceabilityDto>(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));
        }

        var result = await _readService.GetForwardTraceabilityAsync(request.StockLotId, _tenantContext.TenantId);

        if (result is null)
        {
            return Result.Failure<ForwardTraceabilityDto>(Error.NotFound(ErrorCodes.Inventory.ITEM_NOT_FOUND));
        }

        return Result.Success(result);
    }
}