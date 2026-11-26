using CraftFlow.SharedKernel.Constants;
using System.IO;
using System.Text.Json;

namespace CraftFlow.Wpf.Services;

public static class UserSettingsService
{
    private const string APP_FOLDER_NAME = "CraftFlow";
    private const string SETTINGS_FILE_NAME = "user_settings.json";

    private static readonly string _settingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        APP_FOLDER_NAME,
        SETTINGS_FILE_NAME);

    private static UserSettingsModel _current = LoadFromDisk();

    public static string Language => _current.Language;
    public static string Theme => _current.Theme;

    public static void SaveLanguage(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName) || _current.Language == cultureName) return;
        _current = _current with { Language = cultureName };
        SaveToDisk();
    }

    public static void SaveTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName) || _current.Theme == themeName) return;
        _current = _current with { Theme = themeName };
        SaveToDisk();
    }

    private static UserSettingsModel LoadFromDisk()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<UserSettingsModel>(json);
                if (loaded != null)
                {
                    return new UserSettingsModel(
                        string.IsNullOrWhiteSpace(loaded.Language) ? UiConstants.Cultures.RU : loaded.Language,
                        string.IsNullOrWhiteSpace(loaded.Theme) ? UiConstants.Themes.LIGHT : loaded.Theme
                    );
                }
            }
        }
        catch
        {
        }

        return new UserSettingsModel(UiConstants.Cultures.RU, UiConstants.Themes.LIGHT);
    }

    private static void SaveToDisk()
    {
        try
        {
            var dir = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(_current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch
        {
        }
    }

    private sealed record UserSettingsModel(string Language, string Theme);
}