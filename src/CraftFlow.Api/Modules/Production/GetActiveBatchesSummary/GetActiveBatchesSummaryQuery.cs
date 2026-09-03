using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.GetActiveBatchesSummary;

public record GetActiveBatchesSummaryQuery : IRequest<Result<IEnumerable<ActiveBatchSummaryDto>>>;