using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public record AuditLogDto(Guid Id, string EntityName, string Action, string Details, DateTime CreatedAtUtc);

public partial class AuditPage : Page
{
    private List<AuditLogDto> _currentLogs = [];

    public AuditPage()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var logs = await ApiService.Instance.GetAsync<List<AuditLogDto>>(Endpoints.AUDIT_LOGS);
            _currentLogs = logs ?? [];
            AuditDataGrid.ItemsSource = _currentLogs;

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_currentLogs.Count == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("Id;EntityName;Action;Details;CreatedAtUtc");

        foreach (var log in _currentLogs)
        {
            sb.AppendLine($"{log.Id};{log.EntityName};{log.Action};\"{log.Details}\";{log.CreatedAtUtc:yyyy-MM-dd HH:mm:ss}");
        }

        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audit_export.csv");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

        SetStatus($"{UiConstants.Messages.EXPORT_SUCCESS}: {filePath}", Brushes.Green);
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}