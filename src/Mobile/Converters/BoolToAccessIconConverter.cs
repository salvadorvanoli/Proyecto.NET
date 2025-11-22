using System.Globalization;

namespace Mobile.Converters;

public class BoolToAccessIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool wasGranted)
        {
            return wasGranted ? "✅" : "❌";
        }
        return "❓";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
