using CraftFlow.SharedKernel.Dtos.Fermentation;
using CraftFlow.SharedKernel.Enums.Fermrntation;
using CraftFlow.Wpf.Services;
using System.Globalization;

namespace CraftFlow.Wpf.Models.Fermentations;

public sealed class FermentationBatchUiModel
{
    public Guid Id { get; init; }
    public string BatchNumber { get; init; } = string.Empty;
    public string RecipeName { get; init; } = string.Empty;
    public decimal SubstrateWeightKg { get; init; }
    public decimal SaltPercentage { get; init; }
    public decimal CalculatedSaltGrams { get; init; }
    public decimal? CurrentPh { get; init; }
    public string VesselInfo { get; init; } = string.Empty;
    public FermentationStage Stage { get; init; }
    public int DaysElapsed { get; init; }
    public List<FermentationReadingDto> Readings { get; init; } = [];

    public string StageDisplay => Stage switch
    {
        FermentationStage.ACTIVE_WARM => LocalizationService.Get("UI_FERM_STAGE_WARM"),
        FermentationStage.COLD_RETARDATION => LocalizationService.Get("UI_FERM_STAGE_COLD"),
        _ => LocalizationService.Get("UI_FERM_STAGE_DONE")
    };

    public string SaltDisplay => $"{SaltPercentage:0.##}% ({CalculatedSaltGrams:0.#} г)";
    public string PhDisplay => CurrentPh.HasValue ? CurrentPh.Value.ToString("0.00", CultureInfo.InvariantCulture) : "—";
    public bool IsSafeAcidity => CurrentPh.HasValue && CurrentPh.Value < 4.6m;
}