using Microsoft.UI.Xaml.Data;

namespace Win11OptimizationCenter;

public sealed class IconGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value?.ToString() switch
        {
            "Wi-Fi" => "\uE701",
            "Ethernet" => "\uE839",
            _ => "\uE7F4"
        };

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is bool boolean && !boolean;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
