using CraftFlow.SharedKernel.Dtos.Sanitation;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.Wpf.Services;
using System.Windows;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Windows;

public partial class SanitationHistoryWindow : Window
{
    private readonly Guid _targetId;
    private readonly SanitationTargetType _targetType;

    public SanitationHistoryWindow(Guid targetId, string targetName, SanitationTargetType targetType)
    {
        InitializeComponent();
        _targetId = targetId;
        _targetType = targetType;

        var typeTitle = targetType switch
        {
            SanitationTargetType.WAREHOUSE => Get("UI_SANITATION_TARGET_WAREHOUSE"),
            SanitationTargetType.AGING_CHAMBER => Get("UI_SANITATION_TARGET_CHAMBER"),
            _ => Get("UI_SANITATION_TARGET_LOCATION")
        };

        TargetTitleTextBlock.Text = $"{typeTitle}: {targetName}";

        Loaded += async (s, e) => await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            var data = await ApiService.Instance.GetAsync<SanitationDetailsDto>($"api/sanitation/details/{_targetType}/{_targetId}");
            if (data != null)
            {
                StatusTextBlock.Text = $"{Get("UI_SANITATION_LABEL_STATUS")} {data.StatusText}";
                LogsDataGrid.ItemsSource = data.Logs;
            }
        }
        catch (Exception ex)
        {
            AlertWindow.ShowError(this, $"{Get("UI_SANITATION_ERR_LOAD_HISTORY")}: {ex.Message}");
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}