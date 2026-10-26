using System.Globalization;
using System.Windows.Data;

namespace CraftFlow.Wpf.Services.Converters;

public class SupplierStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool isActive && isActive ? "Отключить" : "Включить";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}