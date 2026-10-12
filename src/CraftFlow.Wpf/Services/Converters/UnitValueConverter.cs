using System.Globalization;
using System.Windows.Data;

namespace CraftFlow.Wpf.Services.Converters;
public class UnitValueConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 &&
            values[0] is decimal amount &&
            values[1] is string uomCode)
        {
            return UnitFormatter.Format(amount, uomCode);
        }

        if (values.Length >= 1 && values[0] is decimal singleAmount)
        {
            return $"{singleAmount:0.##}";
        }

        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}