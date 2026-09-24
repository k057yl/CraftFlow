using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.DiscardSpecificAgingItems;

public sealed record DiscardSpecificAgingItemsCommand(
    Guid AgingLotId,
    List<Guid> ItemIds,
    string Reason
) : IRequest<Result>;