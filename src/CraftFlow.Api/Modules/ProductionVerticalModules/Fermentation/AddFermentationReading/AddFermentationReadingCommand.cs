using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.AddFermentationReading;

public record AddFermentationReadingCommand(
    Guid FermentationBatchId,
    decimal? Ph,
    decimal? Temperature,
    bool DegassedOrBurped,
    bool BrineToppedUp,
    string? SensoryNotes
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}