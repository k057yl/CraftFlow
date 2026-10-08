using System.ComponentModel;
using System.Windows.Data;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Localization;

public class Loc : Binding
{
    public Loc()
    {
        Source = TranslationSource.Instance;
        Path = new System.Windows.PropertyPath("Item[]");
        Mode = BindingMode.OneWay;
    }

    public Loc(string key) : this()
    {
        Key = key;
    }

    public string Key
    {
        get => Path.PathParameters.Count > 0 ? (string)Path.PathParameters[0] : string.Empty;
        set => Path = new System.Windows.PropertyPath($"Item[{value}]");
    }
}

public class TranslationSource : INotifyPropertyChanged
{
    public static TranslationSource Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private TranslationSource()
    {
        LocalizationService.LanguageChanged += () =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        };
    }

    public string this[string key]
    {
        get
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                return key;
            }

            return LocalizationService.Get(key);
        }
    }
}