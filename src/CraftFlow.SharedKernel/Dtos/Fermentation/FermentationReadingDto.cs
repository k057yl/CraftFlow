namespace CraftFlow.SharedKernel.Dtos.Fermentation;

public record FermentationReadingDto(
    Guid Id,
    DateTime RecordedAt,
    decimal? Ph,
    decimal? Temperature,
    bool DegassedOrBurped,
    bool BrineToppedUp,
    string? SensoryNotes
);