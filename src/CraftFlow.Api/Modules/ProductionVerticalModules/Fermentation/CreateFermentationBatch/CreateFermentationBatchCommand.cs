using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.CreateFermentationBatch;

public record CreateFermentationBatchCommand(
    Guid ProductionBatchId,
    decimal SubstrateWeightKg,
    decimal SaltPercentage,
    decimal TargetTemperature,
    string VesselInfo,
    decimal? InitialPh = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}