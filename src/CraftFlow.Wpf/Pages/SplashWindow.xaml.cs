using CraftFlow.SharedKernel.Dtos.Dashboard;
using CraftFlow.Wpf.Services.Startup;
using System.Windows;

namespace CraftFlow.Wpf.Pages;

public partial class SplashWindow : Window
{
    private const string ERR_LOAD_FAILED = "ERR_LOAD_FAILED";
    private readonly AppStartupService _startupService = new();

    public SplashWindow()
    {
        InitializeComponent();
        Loaded += OnSplashLoaded;
    }

    private async void OnSplashLoaded(object sender, RoutedEventArgs e)
    {
        var progress = new Progress<StartupProgress>(p =>
        {
            LoadingProgressBar.Value = p.Percent;
            StatusTextBlock.Text = p.Description;
        });

        var result = await _startupService.RunAsync(progress);

        if (result.ResultType == StartupResultType.CriticalError)
        {
            StatusTextBlock.Text = $"{ERR_LOAD_FAILED}: {result.ErrorMessage}";
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

        mainWindow.Loaded += (s, e) =>
        {
            Application.Current.MainWindow = mainWindow;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Close();
        };

        mainWindow.Show();
    }
}