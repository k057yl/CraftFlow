using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.GetAgingLotItems;
public sealed record GetAgingLotItemsQuery(Guid AgingLotId) : IRequest<Result<List<GetAgingLotItemDto>>>;