using System.Windows;
using CraftFlow.Wpf.Services;

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
            UpdateProgress(10, "Проверка локального токена...");

            bool hasSavedToken = ApiService.Instance.IsAuthenticated;
            bool isValidSession = false;

            if (hasSavedToken)
            {
                UpdateProgress(30, "Валидация сессии на сервере...");
                isValidSession = await ApiService.Instance.ValidateAndRefreshCurrentUserAsync();
            }

            if (isValidSession)
            {
                UpdateProgress(70, "Загрузка данных дашборда...");
                await ApiService.Instance.GetAsync<object>("api/dashboard/stats");
            }

            UpdateProgress(100, "Готово!");
            await Task.Delay(150);

            var mainWindow = new MainWindow();

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

            var mainWindow = new MainWindow();
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