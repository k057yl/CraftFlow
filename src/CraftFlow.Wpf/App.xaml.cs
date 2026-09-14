using System.Windows;
using CraftFlow.Wpf.Pages;

namespace CraftFlow.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new SplashWindow();
        splash.Show();
    }
}