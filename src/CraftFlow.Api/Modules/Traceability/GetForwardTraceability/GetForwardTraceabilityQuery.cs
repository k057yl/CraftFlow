using CraftFlow.SharedKernel.Dtos.Traceability;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Traceability.GetForwardTraceability;
public sealed record GetForwardTraceabilityQuery(Guid StockLotId) : IRequest<Result<ForwardTraceabilityDto>>;