using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CraftFlow.Wpf.Services.Converters;

public class SupplierStatusStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isActive = value is bool b && b;
        string key = isActive ? "DangerButtonStyle" : "PrimaryButtonStyle";

        return Application.Current?.FindResource(key) ?? DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}