using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Production.Batches.DiscardBatch;

public record DiscardBatchCommand(Guid BatchId, string Reason) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.INVENTORY_WRITEOFF;
}