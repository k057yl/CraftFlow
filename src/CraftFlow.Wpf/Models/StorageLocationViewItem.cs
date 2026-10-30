using CraftFlow.SharedKernel.Enums.Inventory;
using System.Windows.Media;

namespace CraftFlow.Wpf.Models;

public class StorageLocationViewItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public StorageLocationType LocationType { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public decimal CurrentVolume { get; set; }
    public string VolumeInfo { get; set; } = string.Empty;
    public string LastWashedText { get; set; } = string.Empty;
    public string NextWashText { get; set; } = string.Empty;
    public string SanitationStatusCode { get; set; } = "OK";
    public string SanitationStatusText { get; set; } = "ОК";
    public Brush StatusForeground { get; set; } = Brushes.Green;
    public Brush StatusBackground { get; set; } = Brushes.LightGreen;
}