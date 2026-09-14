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
            UpdateProgress(20, "Проверка авторизации...");
            bool isAuthenticated = ApiService.Instance.IsAuthenticated;

            if (isAuthenticated)
            {
                UpdateProgress(50, "Загрузка дашборда и справочников...");

                await ApiService.Instance.GetAsync<object>("api/dashboard/stats");
            }

            UpdateProgress(100, "Готово!");
            await Task.Delay(150);

            var mainWindow = new MainWindow();
            mainWindow.Show();

            Close();
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Ошибка загрузки: {ex.Message}";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;

            await Task.Delay(2000);
            var mainWindow = new MainWindow();
            mainWindow.NavigateToAuth();
            mainWindow.Show();
            Close();
        }
    }

    private void UpdateProgress(int percent, string status)
    {
        LoadingProgressBar.Value = percent;
        StatusTextBlock.Text = status;
    }
}