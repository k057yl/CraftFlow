namespace CraftFlow.Wpf.Models;
public sealed record AgingLotDto(
    Guid Id,
    string BatchNumber,
    string ChamberName,
    decimal CurrentQuantity,
    string Status,
    DateTime PlacedAt,
    DateTime TargetReleaseDate
);