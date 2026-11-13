using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;

public sealed class FermentationReading : Entity
{
    public Guid FermentationBatchId { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public decimal? Ph { get; private set; }
    public decimal? Temperature { get; private set; }
    public bool DegassedOrBurped { get; private set; }
    public bool BrineToppedUp { get; private set; }
    public string? SensoryNotes { get; private set; }

    private FermentationReading() { }

    public static FermentationReading Create(
        Guid fermentationBatchId,
        decimal? ph,
        decimal? temperature,
        bool degassedOrBurped,
        bool brineToppedUp,
        string? sensoryNotes)
    {
        return new FermentationReading
        {
            Id = Guid.NewGuid(),
            FermentationBatchId = fermentationBatchId,
            RecordedAt = DateTime.UtcNow,
            Ph = ph,
            Temperature = temperature,
            DegassedOrBurped = degassedOrBurped,
            BrineToppedUp = brineToppedUp,
            SensoryNotes = sensoryNotes
        };
    }
}