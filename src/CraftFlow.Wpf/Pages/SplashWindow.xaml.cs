using CraftFlow.Api.Modules.Analytics;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Windows;

namespace CraftFlow.Wpf.Pages;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        Loaded += async (s, e) => await ExecuteRealStartupLoadingAsync();
    }

    private async Task ExecuteRealStartupLoadingAsync()
    {
        try
        {
            UpdateProgress(20, "Проверка авторизации...");

            bool hasSavedToken = ApiService.Instance.IsAuthenticated;
            bool isValidSession = false;

            if (hasSavedToken)
            {
                UpdateProgress(40, "Валидация сессии...");
                isValidSession = await ApiService.Instance.ValidateAndRefreshCurrentUserAsync();
            }

            DashboardSummaryDto? initialSummary = null;

            if (isValidSession)
            {
                UpdateProgress(70, "Загрузка данных дашборда...");
                initialSummary = await ApiService.Instance.GetAsync<DashboardSummaryDto>(AnalyticConstants.DASHBOARD);
            }

            UpdateProgress(100, "Открытие системы...");

            var mainWindow = new MainWindow(initialSummary);

            if (!isValidSession)
            {
                mainWindow.NavigateToAuth();
            }

            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Ошибка загрузки: {ex.Message}";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;

            await Task.Delay(1500);

            var mainWindow = new MainWindow(null);
            mainWindow.NavigateToAuth();
            mainWindow.Show();
            Close();
        }
    }

    private void UpdateProgress(int percent, string status)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingProgressBar.Value = percent;
            StatusTextBlock.Text = status;
        });
    }
}