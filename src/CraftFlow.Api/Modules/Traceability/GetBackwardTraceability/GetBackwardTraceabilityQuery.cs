using CraftFlow.SharedKernel.Dtos.Traceability;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Traceability.GetBackwardTraceability;

public sealed record GetBackwardTraceabilityQuery(Guid ProductStockLotId) : IRequest<Result<BackwardTraceabilityDto>>;