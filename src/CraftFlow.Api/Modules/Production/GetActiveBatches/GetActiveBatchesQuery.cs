using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.GetActiveBatches
{
    public record GetActiveBatchesQuery : IRequest<Result<List<ActiveBatchDto>>>;
}
