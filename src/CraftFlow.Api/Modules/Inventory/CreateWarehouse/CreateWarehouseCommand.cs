using CraftFlow.Api.Common.Behaviors;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.CreateWarehouse;

public record CreateWarehouseCommand(
    string Name,
    string? Address
) : IRequest<Result<Guid>>, IRequireQuotaValidation
{
    public QuotaType QuotaType => QuotaType.WarehousesCount;
}
