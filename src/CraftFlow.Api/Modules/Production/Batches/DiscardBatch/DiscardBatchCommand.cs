using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Batches.DiscardBatch;

public record DiscardBatchCommand(Guid BatchId, string Reason) : IRequest<Result>;