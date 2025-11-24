using System.Globalization;

namespace Mobile.Converters;

/// <summary>
/// Converts UTC DateTime to local time (Uruguay timezone UTC-3) for display.
/// </summary>
public class UtcToLocalDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            // Si la fecha está en UTC, convertir a hora local del dispositivo
            // El dispositivo debe tener configurado el huso horario de Uruguay
            DateTime localTime = dateTime.Kind == DateTimeKind.Utc 
                ? dateTime.ToLocalTime() 
                : dateTime;
            
            // Obtener el formato del parámetro o usar formato por defecto
            string format = parameter?.ToString() ?? "dd/MM/yyyy HH:mm:ss";
            
            return localTime.ToString(format, CultureInfo.InvariantCulture);
        }
        
        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("UtcToLocalDateTimeConverter does not support ConvertBack");
    }
}
