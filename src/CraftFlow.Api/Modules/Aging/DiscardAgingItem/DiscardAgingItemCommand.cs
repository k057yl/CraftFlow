using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.DiscardAgingItem;

public sealed record DiscardAgingItemCommand(
    Guid AgingLotId,
    List<Guid> ItemIds,
    string Reason
) : IRequest<Result>;