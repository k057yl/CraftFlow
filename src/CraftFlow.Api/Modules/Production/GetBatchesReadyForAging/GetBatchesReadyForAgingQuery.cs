using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.GetBatchesReadyForAging;
public record GetBatchesReadyForAgingQuery : IRequest<Result<List<BatchReadyForAgingDto>>>;