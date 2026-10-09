using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Modules.Analytics;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Analytics;
using CraftFlow.Wpf.Services;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class AuditPage : Page
{
    private List<AuditLogDto> _currentLogs = [];

    public AuditPage()
    {
        InitializeComponent();
        InitFilters();
        Loaded += async (s, e) => await LoadDataAsync();
    }

    private void InitFilters()
    {
        ActionComboBox.ItemsSource = new List<string> { "Все", "Added", "Modified", "Deleted" };
        ActionComboBox.SelectedIndex = 0;
        FromDatePicker.SelectedDate = DateTime.Now.AddDays(-7);
        ToDatePicker.SelectedDate = DateTime.Now;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTextBox.Text.Trim())}");

            if (ActionComboBox.SelectedIndex > 0 && ActionComboBox.SelectedItem is string action)
                queryParams.Add($"action={action}");

            if (FromDatePicker.SelectedDate.HasValue)
                queryParams.Add($"fromDate={FromDatePicker.SelectedDate.Value:yyyy-MM-dd}");

            if (ToDatePicker.SelectedDate.HasValue)
                queryParams.Add($"toDate={ToDatePicker.SelectedDate.Value:yyyy-MM-dd}");

            var url = $"{AnalyticConstants.AUDIT_LOGS}?{string.Join("&", queryParams)}";

            var logs = await ApiService.Instance.GetAsync<List<AuditLogDto>>(url);
            _currentLogs = logs ?? [];
            AuditDataGrid.ItemsSource = _currentLogs;

            SetStatus($"{UiConstants.Messages.DATA_LOADED_SUCCESS} (Записей: {_currentLogs.Count})", Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void ApplyFilter_Click(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async void ResetFilter_Click(object sender, RoutedEventArgs e)
    {
        SearchTextBox.Text = string.Empty;
        ActionComboBox.SelectedIndex = 0;
        FromDatePicker.SelectedDate = DateTime.Now.AddDays(-7);
        ToDatePicker.SelectedDate = DateTime.Now;
        await LoadDataAsync();
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_currentLogs.Count == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("CreatedAtUtc;UserEmail;Action;Details");

        foreach (var log in _currentLogs)
        {
            var detailsEscaped = log.Details?.Replace("\"", "\"\"") ?? string.Empty;
            sb.AppendLine($"{log.CreatedAtUtc:yyyy-MM-dd HH:mm:ss};{log.UserEmail};{log.Action};\"{detailsEscaped}\"");
        }

        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audit_export.csv");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

        SetStatus($"{UiConstants.Messages.EXPORT_SUCCESS}: {filePath}", Brushes.Green);
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = msg;
    }
}