using CraftFlow.SharedKernel.Dtos.Dashboard;
using CraftFlow.Wpf.Services.Startup;
using System.Windows;

namespace CraftFlow.Wpf.Pages;

public partial class SplashWindow : Window
{
    private readonly AppStartupService _startupService = new();

    public SplashWindow()
    {
        InitializeComponent();
        Loaded += async (s, e) => await StartLoadingAsync();
    }

    private async Task StartLoadingAsync()
    {
        var progress = new Progress<StartupProgress>(p =>
        {
            LoadingProgressBar.Value = p.Percent;
            StatusTextBlock.Text = p.Description;
        });

        var result = await _startupService.RunAsync(progress);

        if (result.ResultType == StartupResultType.CriticalError)
        {
            StatusTextBlock.Text = $"Ошибка загрузки: {result.ErrorMessage}";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            await Task.Delay(1500);
        }

        OpenMainApp(result);
    }

    private void OpenMainApp(StartupResult result)
    {
        var summary = result.InitialData as DashboardSummaryDto;
        var mainWindow = new MainWindow(summary);

        if (result.ResultType != StartupResultType.Success)
        {
            mainWindow.NavigateToAuth();
        }

        mainWindow.Show();
        Application.Current.MainWindow = mainWindow;
        Close();
    }
}