using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.MRP.CreateProcurement;

public sealed record CreateProcurementFromMrpCommand(
    Guid SupplierId,
    Guid WarehouseId,
    List<MrpPurchaseItemRequestDto> Items
) : IRequest<Result<Guid>>;