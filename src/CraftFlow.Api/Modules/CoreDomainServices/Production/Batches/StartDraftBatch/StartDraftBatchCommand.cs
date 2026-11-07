using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Production.Batches.StartDraftBatch;

public sealed record StartDraftBatchCommand(Guid BatchId) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}