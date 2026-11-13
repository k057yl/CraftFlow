namespace CraftFlow.SharedKernel.Dtos.Fermentation;

public record AddFermentationReadingRequest(
    decimal? Ph,
    decimal? Temperature,
    bool DegassedOrBurped,
    bool BrineToppedUp,
    string? SensoryNotes
);