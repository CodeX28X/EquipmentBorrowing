using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.Converters;

public sealed class EquipmentDisplayConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not Equipment equipment)
            return string.Empty;

        string text =
            $"{equipment.EquipmentId} - {equipment.EquipmentName}";

        if (!equipment.IsAvailable)
        {
            text += " (Taken)";
        }

        return text;
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