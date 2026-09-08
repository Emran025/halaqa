using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Halaqa.Desktop.Shared.Presentation.Converters;

public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasContent = value switch
        {
            string str => !string.IsNullOrWhiteSpace(str),
            null => false,
            _ => true
        };

        return hasContent ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
