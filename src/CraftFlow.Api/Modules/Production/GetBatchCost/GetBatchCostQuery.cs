using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.GetBatchCost;
public record GetBatchCostQuery(Guid BatchId) : IRequest<Result<BatchCostDto>>;
