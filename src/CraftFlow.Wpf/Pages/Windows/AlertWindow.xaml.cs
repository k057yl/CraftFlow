using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Windows;

public enum AlertType
{
    Info,
    Success,
    Warning,
    Error
}

public partial class AlertWindow : Window
{
    public AlertWindow(string title, string message, AlertType type = AlertType.Error)
    {
        InitializeComponent();

        TitleTextBlock.Text = title;
        MessageTextBlock.Text = message;

        ConfigureType(type);
    }

    private void ConfigureType(AlertType type)
    {
        switch (type)
        {
            case AlertType.Success:
                IconTextBlock.Text = "✅";
                IconTextBlock.Foreground = Brushes.Green;
                break;
            case AlertType.Warning:
                IconTextBlock.Text = "⚠️";
                IconTextBlock.Foreground = Brushes.Orange;
                break;
            case AlertType.Error:
                IconTextBlock.Text = "❌";
                IconTextBlock.Foreground = Brushes.Red;
                break;
            default:
                IconTextBlock.Text = "ℹ️";
                IconTextBlock.Foreground = (Brush)Application.Current.Resources["AccentBrush"];
                break;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    public static void ShowError(Window owner, string rawOrKeyMessage, string? title = null)
    {
        var localizedTitle = title ?? LocalizationService.Get(UiConstants.Dialogs.TITLE_ERROR);
        var parsedMessage = ParseAndLocalizeError(rawOrKeyMessage);

        var win = new AlertWindow(localizedTitle, parsedMessage, AlertType.Error) { Owner = owner };
        win.ShowDialog();
    }

    public static void ShowSuccess(Window owner, string rawOrKeyMessage, string? title = null)
    {
        var localizedTitle = title ?? LocalizationService.Get(UiConstants.Dialogs.TITLE_SUCCESS);
        var parsedMessage = ParseAndLocalizeError(rawOrKeyMessage);

        var win = new AlertWindow(localizedTitle, parsedMessage, AlertType.Success) { Owner = owner };
        win.ShowDialog();
    }

    private static string ParseAndLocalizeError(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        try
        {
            var trimmed = input.TrimStart();
            if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
            {
                using var doc = JsonDocument.Parse(input);
                var root = doc.RootElement;
                var localizedErrors = new List<string>();

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("code", out var codeProp) && codeProp.GetString() is string code)
                    {
                        localizedErrors.Add(ResolveKey(code));
                    }
                    else if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in errorsProp.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var errItem in prop.Value.EnumerateArray())
                                {
                                    if (errItem.GetString() is string errCode)
                                    {
                                        localizedErrors.Add(ResolveKey(errCode));
                                    }
                                }
                            }
                        }
                    }
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var elem in root.EnumerateArray())
                    {
                        if (elem.ValueKind == JsonValueKind.Object && elem.TryGetProperty("code", out var codeProp) && codeProp.GetString() is string code)
                        {
                            localizedErrors.Add(ResolveKey(code));
                        }
                    }
                }

                if (localizedErrors.Count > 0)
                {
                    return string.Join("\n", localizedErrors.Distinct());
                }
            }
        }
        catch
        {
        }

        return ResolveKey(input);
    }

    private static string ResolveKey(string rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey)) return string.Empty;

        var localized = LocalizationService.Get(rawKey);
        if (localized != rawKey) return localized;

        var normalizedKey = rawKey.Replace('.', '_');
        localized = LocalizationService.Get(normalizedKey);
        if (localized != normalizedKey) return localized;

        var shortKey = rawKey.Split('.', '_').Last();
        localized = LocalizationService.Get(shortKey);

        return localized;
    }
}