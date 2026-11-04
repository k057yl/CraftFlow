using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CraftFlow.SharedKernel.Dtos.Dashboard;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Services.Startup;

namespace CraftFlow.Wpf.Pages;

public partial class SplashWindow : Window
{
    private const string ERR_LOAD_FAILED = "ERR_LOAD_FAILED";
    private const string ERR_UNEXPECTED = "ERR_UNEXPECTED";
    private const int DELAY_ON_ERROR_MS = 2500;
    private const int DELAY_ON_COMPLETE_MS = 600;
    private const int ANIMATION_DURATION_MS = 400;

    private readonly AppStartupService _startupService = new();

    public SplashWindow()
    {
        InitializeComponent();
        Loaded += OnSplashLoaded;
    }

    private async void OnSplashLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var progress = new Progress<StartupProgress>(p =>
            {
                AnimateProgress(p.Percent);
                StatusTextBlock.Text = p.Description;
            });

            var result = await _startupService.RunAsync(progress);

            if (result.ResultType == StartupResultType.CriticalError)
            {
                StatusTextBlock.Text = $"{LocalizationService.Get(ERR_LOAD_FAILED)}: {result.ErrorMessage}";
                StatusTextBlock.Foreground = Brushes.Red;
                await Task.Delay(DELAY_ON_ERROR_MS);

                Application.Current.Shutdown();
                return;
            }

            AnimateProgress(100);
            await Task.Delay(DELAY_ON_COMPLETE_MS);

            OpenMainApp(result);
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"{LocalizationService.Get(ERR_UNEXPECTED)}: {ex.Message}";
            StatusTextBlock.Foreground = Brushes.Red;
            await Task.Delay(DELAY_ON_ERROR_MS);

            Application.Current.Shutdown();
        }
    }

    private CancellationTokenSource? _progressCts;

    private async void AnimateProgress(int targetPercent)
    {
        _progressCts?.Cancel();
        _progressCts = new CancellationTokenSource();
        var token = _progressCts.Token;

        double startPercent = LoadingProgressBar.Value;
        if (Math.Abs(startPercent - targetPercent) < 0.5) return;

        var animation = new DoubleAnimation
        {
            From = startPercent,
            To = targetPercent,
            Duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION_MS),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        LoadingProgressBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, animation);

        try
        {
            int steps = 15;
            int stepDelay = ANIMATION_DURATION_MS / steps;

            for (int i = 1; i <= steps; i++)
            {
                await Task.Delay(stepDelay, token);

                double current = startPercent + (targetPercent - startPercent) * (i / (double)steps);
                PercentTextBlock.Text = $"{Math.Round(current)}%";
            }

            PercentTextBlock.Text = $"{targetPercent}%";
        }
        catch (TaskCanceledException)
        {
        }
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