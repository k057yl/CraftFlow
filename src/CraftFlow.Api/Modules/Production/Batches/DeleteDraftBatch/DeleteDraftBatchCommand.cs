using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Batches.DeleteDraftBatch;

public sealed record DeleteDraftBatchCommand(Guid BatchId) : IRequest<Result>;