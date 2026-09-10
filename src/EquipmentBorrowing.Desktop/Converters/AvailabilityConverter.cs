using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EquipmentBorrowing.Desktop.Converters;

public sealed class AvailabilityConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not bool isAvailable)
            return string.Empty;

        return isAvailable
            ? "Available"
            : "Unavailable";
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}