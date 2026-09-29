using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using CraftFlow.Wpf.Resources;

namespace CraftFlow.Wpf.Localization;

[MarkupExtensionReturnType(typeof(string))]
public class Loc : MarkupExtension
{
    private const string EMPTY_KEY_RESULT = "";

    [ConstructorArgument("key")]
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
        if (string.IsNullOrEmpty(Key)) return EMPTY_KEY_RESULT;

        if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
        {
            return Key;
        }

        try
        {
            var resourceManager = new ResourceManager(typeof(Strings));
            var localizedString = resourceManager.GetString(Key, CultureInfo.CurrentUICulture);

            return localizedString ?? Key;
        }
        catch
        {
            return Key;
        }
    }
}