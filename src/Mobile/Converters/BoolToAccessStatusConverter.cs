using System.Globalization;

namespace Mobile.Converters;

public class BoolToAccessStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool wasGranted)
        {
            return wasGranted ? "Acceso Permitido" : "Acceso Denegado";
        }
        return "Estado Desconocido";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
