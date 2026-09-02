using System.Collections.Concurrent;
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
    private static readonly Uri ApplicationBaseUri = new("pack://application:,,,/");
    private static readonly FontFamily FallbackQuranFont = CreateFontFamily(
        "./Assets/Fonts/UthmanicHafs_V20.ttf#KFGQPC HAFS Uthmanic Script");
    private static readonly ConcurrentDictionary<int, FontFamily> PageFonts = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Halaqa.Desktop.Features.Quran.Domain.Entities.QuranPage page)
        {
            return page.IsFromLocalCache
                ? GetPageFont(page.PageNumber)
                : FallbackQuranFont;
        }

        if (value is int pageNumber)
        {
            return GetPageFont(pageNumber);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException("لا يمكن تحويل عائلة خط المصحف إلى رقم صفحة.");

    private static FontFamily GetPageFont(int pageNumber)
    {
        if (pageNumber is < FirstPage or > LastPage)
        {
            return FallbackQuranFont;
        }

        return PageFonts.GetOrAdd(pageNumber, static page =>
        {
            var fontNumber = page + FontFileOffset;
            return CreateFontFamily(
                $"./Assets/Fonts/QuranPages/p{fontNumber}.ttf#QCF{fontNumber}");
        });
    }

    private static FontFamily CreateFontFamily(string fontReference) =>
        new(ApplicationBaseUri, fontReference);
}
