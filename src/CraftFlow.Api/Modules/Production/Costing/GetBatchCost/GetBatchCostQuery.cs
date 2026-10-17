using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Costing.GetBatchCost;
public record GetBatchCostQuery(Guid BatchId) : IRequest<Result<BatchCostDto>>;
