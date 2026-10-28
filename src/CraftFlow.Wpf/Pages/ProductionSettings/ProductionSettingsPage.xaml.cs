using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages.ProductionSettings;

public partial class ProductionSettingsPage : Page
{
    public ProductionSettingsPage() : this(null)
    {
    }

    public ProductionSettingsPage(object? dummy = null)
    {
        InitializeComponent();
    }

    private void OnStatusUpdated(string message, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = message;
    }
}