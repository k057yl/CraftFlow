namespace CraftFlow.SharedKernel.Dtos.Sale;

public sealed record LotItemDetailsDto(
    Guid Id,
    string ItemNumber,
    decimal CurrentWeight
);