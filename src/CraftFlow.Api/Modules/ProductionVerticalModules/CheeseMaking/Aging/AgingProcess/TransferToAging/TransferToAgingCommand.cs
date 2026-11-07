using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.TransferToAging;

public record TransferToAgingRequest(
    Guid ProductionBatchId,
    Guid AgingChamberId,
    int MinAgingDays,
    int UnitsCount = 1,
    string? CustomBatchNumber = null,
    Guid? StorageLocationId = null
);

public record TransferToAgingCommand(
    Guid ProductionBatchId,
    Guid AgingChamberId,
    int MinAgingDays,
    int UnitsCount = 1,
    string? CustomBatchNumber = null,
    Guid? StorageLocationId = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}