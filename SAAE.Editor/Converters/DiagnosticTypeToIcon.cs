﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SAAE.Editor.Models.Compilation;

namespace SAAE.Editor.Converters;

public class DiagnosticTypeToIcon : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DiagnosticType diagnosticType)
        {
            return null;
        }
        return diagnosticType switch
        {
            DiagnosticType.Warning => "\ue4e0",
            DiagnosticType.Error => "\ue4e2",
            DiagnosticType.Information => "\ue2ce",
            DiagnosticType.Unknown => "\ue3e8",
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DiagnosticTypeToColor : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DiagnosticType diagnosticType)
        {
            return null;
        }
        return diagnosticType switch
        {
            DiagnosticType.Warning => new ImmutableSolidColorBrush(Color.FromRgb(237,206,7)),
            DiagnosticType.Error => new ImmutableSolidColorBrush(Color.FromRgb(209,11,4)),
            DiagnosticType.Information => new ImmutableSolidColorBrush(Color.FromRgb(255,255,255)),
            DiagnosticType.Unknown => new ImmutableSolidColorBrush(Color.FromRgb(255,255,255)),
            _ => null // Default case if needed
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}