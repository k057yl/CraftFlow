using System.Globalization;
using System.Resources;
using CraftFlow.Wpf.Resources;

namespace CraftFlow.Wpf.Services;

public static class LocalizationService
{
    private static readonly ResourceManager _resourceManager = new(typeof(Strings));

    public static string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var parts = key.Split(' ', 2);
        var normalizedKey = parts[0].Replace('.', '_');

        var localized = _resourceManager.GetString(normalizedKey, CultureInfo.CurrentUICulture);

        if (localized != null)
        {
            return parts.Length > 1 ? $"{localized} ({parts[1]})" : localized;
        }

        return key;
    }

    public static void SetCulture(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}