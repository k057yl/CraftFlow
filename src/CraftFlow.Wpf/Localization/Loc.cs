using System.Globalization;
using System.Resources;
using System.Windows.Markup;
using CraftFlow.Wpf.Resources;

namespace CraftFlow.Wpf.Localization;

[MarkupExtensionReturnType(typeof(string))]
public class Loc : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public Loc()
    {
    }

    public Loc(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key)) return string.Empty;

        var resourceManager = new ResourceManager(typeof(Strings));
        var localizedString = resourceManager.GetString(Key, CultureInfo.CurrentUICulture);

        return localizedString ?? Key;
    }
}