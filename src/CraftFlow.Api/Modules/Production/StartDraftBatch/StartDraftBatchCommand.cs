using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.StartDraftBatch;

public sealed record StartDraftBatchCommand(Guid BatchId) : IRequest<Result>;