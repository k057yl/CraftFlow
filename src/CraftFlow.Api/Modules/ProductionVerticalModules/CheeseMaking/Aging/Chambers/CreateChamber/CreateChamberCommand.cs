using CraftFlow.Api.Core.Authorization;
using CraftFlow.Api.Core.Behaviors;
using CraftFlow.Api.Core.Enums;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Chambers.CreateChamber;

public record CreateChamberCommand(
    string Name,
    decimal TargetTemperature,
    decimal TargetHumidity
) : IRequest<Result<Guid>>, IRequireQuotaValidation, IRequirePermission, IRequireModule
{
    public QuotaType QuotaType => QuotaType.CHAMBER_COUNT;
    public AppPermission RequiredPermission => AppPermission.WAREHOUSES_MANAGE;
    public AppModule RequiredModule => AppModule.CHEESE_MAKING;
}