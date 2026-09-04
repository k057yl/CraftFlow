using System;
using System.Linq;
using System.Windows;

namespace CraftFlow.Wpf.Services;

public static class ThemeService
{
    public static void SetTheme(string themeName)
    {
        var appResources = Application.Current.Resources;

        var oldTheme = appResources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.EndsWith("Theme.xaml", StringComparison.OrdinalIgnoreCase));

        if (oldTheme != null)
        {
            appResources.MergedDictionaries.Remove(oldTheme);
        }

        var themeUri = new Uri($"Themes/{themeName}Theme.xaml", UriKind.Relative);
        var newThemeDict = new ResourceDictionary { Source = themeUri };

        appResources.MergedDictionaries.Add(newThemeDict);
    }
}