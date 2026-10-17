using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Batches.GetActiveBatchesSummary;

public record GetActiveBatchesSummaryQuery : IRequest<Result<IEnumerable<ActiveBatchSummaryDto>>>;