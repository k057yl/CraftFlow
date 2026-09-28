using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.DeleteDraftBatch;

public sealed record DeleteDraftBatchCommand(Guid BatchId) : IRequest<Result>;