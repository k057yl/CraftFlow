using CraftFlow.Api.Common.Behaviors;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.CreateChamber;

public record CreateChamberCommand(
    string Name,
    decimal TargetTemperature,
    decimal TargetHumidity
) : IRequest<Result<Guid>>, IRequireQuotaValidation
{
    public QuotaType QuotaType => QuotaType.ChamberCount;
}