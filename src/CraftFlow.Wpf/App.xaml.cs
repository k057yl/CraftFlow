using System.Windows;
using CraftFlow.Wpf.Pages;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LocalizationService.Initialize();
        ThemeService.Initialize();

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var splash = new SplashWindow();
        splash.Show();
    }
}