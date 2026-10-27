using CraftFlow.Api.Common.Authorization;
using CraftFlow.Api.Common.Behaviors;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.Chambers.CreateChamber;

public record CreateChamberCommand(
    string Name,
    decimal TargetTemperature,
    decimal TargetHumidity
) : IRequest<Result<Guid>>, IRequireQuotaValidation, IRequirePermission
{
    public QuotaType QuotaType => QuotaType.ChamberCount;
    public AppPermission RequiredPermission => AppPermission.WAREHOUSES_MANAGE;
}