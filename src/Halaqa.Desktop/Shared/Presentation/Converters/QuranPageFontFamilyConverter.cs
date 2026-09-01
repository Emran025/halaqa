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
    private static readonly FontFamily FallbackQuranFont = new(
        "pack://application:,,,/Assets/Fonts/UthmanicHafs_V20.ttf#KFGQPC HAFS Uthmanic Script");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Halaqa.Desktop.Features.Quran.Domain.Entities.QuranPage page)
        {
            if (!page.IsFromLocalCache)
            {
                return FallbackQuranFont;
            }

            var fontNumber = page.PageNumber + FontFileOffset;
            return new FontFamily(
                $"pack://application:,,,/Assets/Fonts/QuranPages/p{fontNumber}.ttf#QCF{fontNumber}");
        }

        if (value is int pageNumber && pageNumber is >= FirstPage and <= LastPage)
        {
            var fontNumber = pageNumber + FontFileOffset;
            return new FontFamily(
                $"pack://application:,,,/Assets/Fonts/QuranPages/p{fontNumber}.ttf#QCF{fontNumber}");
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException("لا يمكن تحويل عائلة خط المصحف إلى رقم صفحة.");
}
