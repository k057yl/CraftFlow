using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Procurement.PurchaseOrders;

public sealed record ReceiveGoodsCommand(Guid PurchaseOrderId) : IRequest<Result>;