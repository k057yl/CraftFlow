using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.DiscardFermentationBatch;

public sealed record DiscardFermentationBatchCommand(
    Guid FermentationBatchId,
    string Reason
) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.INVENTORY_WRITEOFF;
}