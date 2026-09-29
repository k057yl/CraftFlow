using System.Globalization;
using System.Resources;
using CraftFlow.Wpf.Resources;

namespace CraftFlow.Wpf.Services;

public static class LocalizationService
{
    private const char KEY_SPLIT_CHAR = ' ';
    private const char KEY_REPLACE_OLD = '.';
    private const char KEY_REPLACE_NEW = '_';
    private const string FORMAT_EXTRA_PART = "{0} ({1})";

    private static readonly ResourceManager _resourceManager = new(typeof(Strings));

    public static CultureInfo CurrentCulture => CultureInfo.CurrentUICulture;

    public static event Action? LanguageChanged;

    public static string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var parts = key.Split(KEY_SPLIT_CHAR, 2);
        var normalizedKey = parts[0].Replace(KEY_REPLACE_OLD, KEY_REPLACE_NEW);

        var localized = _resourceManager.GetString(normalizedKey, CultureInfo.CurrentUICulture);

        if (localized != null)
        {
            return parts.Length > 1 ? string.Format(FORMAT_EXTRA_PART, localized, parts[1]) : localized;
        }

        return key;
    }

    public static void SetCulture(string cultureName)
    {
        var culture = new CultureInfo(cultureName);

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        LanguageChanged?.Invoke();
    }
}