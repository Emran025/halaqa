using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Halaqa.Desktop.Features.Memberships.Domain.Entities;

namespace Halaqa.Desktop.Shared.Presentation.Converters;

public sealed class MembershipStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MembershipStatus status)
        {
            return status switch
            {
                MembershipStatus.Active => "نشط",
                MembershipStatus.Inactive => "غير نشط",
                MembershipStatus.Removed => "مزال",
                _ => value.ToString() ?? string.Empty
            };
        }

        if (value is string str)
        {
            return str.ToLowerInvariant() switch
            {
                "active" => "نشط",
                "inactive" => "غير نشط",
                "removed" => "مزال",
                _ => str
            };
        }

        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class MembershipStatusToBackgroundBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush ActiveBrush = new(Color.FromRgb(0xDF, 0xF6, 0xE9)); // AppSuccessSoftBrush
    private static readonly SolidColorBrush InactiveBrush = new(Color.FromRgb(0xFF, 0xF1, 0xCF)); // AppWarningSoftBrush
    private static readonly SolidColorBrush RemovedBrush = new(Color.FromRgb(0xFD, 0xE7, 0xE5)); // AppErrorSoftBrush
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0xF0, 0xF3, 0xF5)); // AppSurfaceMutedBrush

    static MembershipStatusToBackgroundBrushConverter()
    {
        ActiveBrush.Freeze();
        InactiveBrush.Freeze();
        RemovedBrush.Freeze();
        DefaultBrush.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var key = value switch
        {
            MembershipStatus status => status.ToString().ToLowerInvariant(),
            string str => str.ToLowerInvariant(),
            _ => null
        };

        return key switch
        {
            "active" => ActiveBrush,
            "inactive" => InactiveBrush,
            "removed" => RemovedBrush,
            _ => DefaultBrush
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class MembershipStatusToForegroundBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush ActiveBrush = new(Color.FromRgb(0x14, 0x7A, 0x4B)); // AppSuccessBrush
    private static readonly SolidColorBrush InactiveBrush = new(Color.FromRgb(0x9B, 0x62, 0x00)); // AppWarningBrush
    private static readonly SolidColorBrush RemovedBrush = new(Color.FromRgb(0xB4, 0x23, 0x18)); // AppErrorBrush
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0x26, 0x31, 0x3B)); // AppMutedInkBrush

    static MembershipStatusToForegroundBrushConverter()
    {
        ActiveBrush.Freeze();
        InactiveBrush.Freeze();
        RemovedBrush.Freeze();
        DefaultBrush.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var key = value switch
        {
            MembershipStatus status => status.ToString().ToLowerInvariant(),
            string str => str.ToLowerInvariant(),
            _ => null
        };

        return key switch
        {
            "active" => ActiveBrush,
            "inactive" => InactiveBrush,
            "removed" => RemovedBrush,
            _ => DefaultBrush
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
