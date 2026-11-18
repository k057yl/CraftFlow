using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Models.Fermentations;

public sealed class RequirementItemUiModel
{
    public string MaterialName { get; set; } = string.Empty;
    public decimal RequiredQty { get; set; }
    public decimal AvailableQty { get; set; }
    public bool IsSufficient { get; set; }

    public string FormattedInfo =>
        $"{MaterialName}: {LocalizationService.Get("UI_BREWHOUSE_LABEL_REQ_NEED")} {RequiredQty:0.####} | {LocalizationService.Get("UI_BREWHOUSE_LABEL_REQ_AVAIL")} {AvailableQty:0.####} {(IsSufficient ? "✔" : "❌")}";
}