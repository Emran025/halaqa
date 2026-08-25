using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Halaqa.Desktop.Shared.Presentation.Converters;

public sealed class QuranPageFontFamilyConverter : IValueConverter
{
    private const int FirstPage = 1;
    private const int LastPage = 604;
    private const int FontFileOffset = 2000;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int pageNumber || pageNumber is < FirstPage or > LastPage)
        {
            return DependencyProperty.UnsetValue;
        }

        var fontNumber = pageNumber + FontFileOffset;
        return new FontFamily(
            $"pack://application:,,,/Assets/Fonts/QuranPages/p{fontNumber}.ttf#QCF{fontNumber}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException("لا يمكن تحويل عائلة خط المصحف إلى رقم صفحة.");
}
