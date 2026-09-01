using System.Threading;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Halaqa.Desktop.Tests.Shared.Presentation;

public sealed class TypographyResourceTests
{
    [Fact]
    public void TypographyResource_LoadsEmbeddedQuranFont()
    {
        Exception? failure = null;
        FontFamily? fontFamily = null;
        var thread = new Thread(() =>
        {
            try
            {
                var dictionary = (ResourceDictionary)Application.LoadComponent(new Uri(
                    "/Halaqa.Desktop;component/Shared/Presentation/Themes/Typography.xaml",
                    UriKind.Relative));
                fontFamily = Assert.IsType<FontFamily>(dictionary["QuranFontFamily"]);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.NotNull(fontFamily);
        Assert.Equal(
            "pack://application:,,,/Assets/Fonts/UthmanicHafs_V20.ttf#KFGQPC HAFS Uthmanic Script",
            fontFamily.ToString());
    }

    [Fact]
    public void QuranPageFontFamilyConverter_CanResolvePage1Typeface()
    {
        var converter = new Halaqa.Desktop.Shared.Presentation.Converters.QuranPageFontFamilyConverter();
        var fontFamily = converter.Convert(1, typeof(FontFamily), null!, System.Globalization.CultureInfo.InvariantCulture) as FontFamily;
        Assert.NotNull(fontFamily);
        Assert.NotNull(fontFamily!.Source);
    }
}
