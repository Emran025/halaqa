﻿using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Halaqa.Desktop.Shared.Presentation.Converters;

public sealed class QuranPageFontFamilyConverter : IValueConverter
{
    private const int FirstPage = 1;
    private const int LastPage = 604;
    private const int FontFileOffset = 2000;
    
    private static readonly string DiskFontsPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "QuranPages");
    private static readonly Uri? DiskFontsUri = Directory.Exists(DiskFontsPath) ? new Uri(DiskFontsPath + Path.DirectorySeparatorChar) : null;
    
    private static readonly FontFamily FallbackQuranFont = new(
        "pack://application:,,,/Halaqa.Desktop;component/Assets/Fonts/#KFGQPC HAFS Uthmanic Script, ./Assets/Fonts/#KFGQPC HAFS Uthmanic Script, KFGQPC HAFS Uthmanic Script");
        
    private static readonly ConcurrentDictionary<int, FontFamily> PageFonts = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Halaqa.Desktop.Features.Quran.Domain.Entities.QuranPage page)
        {
            return GetPageFont(page.PageNumber);
        }

        if (value is int pageNumber)
        {
            return GetPageFont(pageNumber);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException("لا يمكن تحويل عائلة خط المصحف إلى رقم صفحة.");

    public static FontFamily GetPageFont(int pageNumber)
    {
        if (pageNumber is < FirstPage or > LastPage)
        {
            return FallbackQuranFont;
        }

        return PageFonts.GetOrAdd(pageNumber, static page =>
        {
            var fontNumber = page + FontFileOffset;
            
            if (DiskFontsUri != null && File.Exists(Path.Combine(DiskFontsPath, $"p{fontNumber}.ttf")))
            {
                return new FontFamily(DiskFontsUri, $"./#QCF{fontNumber}");
            }
            
            var devPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "Halaqa.Desktop", "Assets", "Fonts", "QuranPages");
            if (Directory.Exists(devPath) && File.Exists(Path.Combine(devPath, $"p{fontNumber}.ttf")))
            {
                return new FontFamily(new Uri(devPath + Path.DirectorySeparatorChar), $"./#QCF{fontNumber}");
            }

            return new FontFamily($"pack://application:,,,/Halaqa.Desktop;component/Assets/Fonts/QuranPages/#QCF{fontNumber}, ./Assets/Fonts/QuranPages/#QCF{fontNumber}, QCF{fontNumber}");
        });
    }
}
