namespace CraftFlow.SharedKernel.Dtos.Aging;

public sealed record GetAgingLotItemDto(
    Guid Id,
    string ItemNumber,
    decimal InitialWeight,
    decimal CurrentWeight,
    int State,
    string? DiscardReason
);