using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Halaqa.Desktop.Features.Registrations.Domain.Entities;

namespace Halaqa.Desktop.Shared.Presentation.Converters;

public sealed class RegistrationStateToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            RegistrationState.Pending => "قيد الانتظار",
            RegistrationState.Accepted => "مقبول",
            RegistrationState.Rejected => "مرفوض",
            RegistrationState.CompletionRequested => "استكمال بيانات",
            RegistrationState.Withdrawn => "مسحوب",
            RegistrationState.Cancelled => "ملغى",
            _ => value?.ToString() ?? string.Empty
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class RegistrationStateToBackgroundBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush PendingBrush = new(Color.FromRgb(0xFF, 0xF1, 0xCF)); // AppWarningSoftBrush
    private static readonly SolidColorBrush AcceptedBrush = new(Color.FromRgb(0xDF, 0xF6, 0xE9)); // AppSuccessSoftBrush
    private static readonly SolidColorBrush RejectedBrush = new(Color.FromRgb(0xFD, 0xE7, 0xE5)); // AppErrorSoftBrush
    private static readonly SolidColorBrush CompletionBrush = new(Color.FromRgb(0xE8, 0xF1, 0xFC)); // AppInfoSoftBrush
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0xF0, 0xF3, 0xF5)); // AppSurfaceMutedBrush

    static RegistrationStateToBackgroundBrushConverter()
    {
        PendingBrush.Freeze();
        AcceptedBrush.Freeze();
        RejectedBrush.Freeze();
        CompletionBrush.Freeze();
        DefaultBrush.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            RegistrationState.Pending => PendingBrush,
            RegistrationState.Accepted => AcceptedBrush,
            RegistrationState.Rejected => RejectedBrush,
            RegistrationState.CompletionRequested => CompletionBrush,
            _ => DefaultBrush
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class RegistrationStateToForegroundBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush PendingBrush = new(Color.FromRgb(0x9B, 0x62, 0x00)); // AppWarningBrush
    private static readonly SolidColorBrush AcceptedBrush = new(Color.FromRgb(0x14, 0x7A, 0x4B)); // AppSuccessBrush
    private static readonly SolidColorBrush RejectedBrush = new(Color.FromRgb(0xB4, 0x23, 0x18)); // AppErrorBrush
    private static readonly SolidColorBrush CompletionBrush = new(Color.FromRgb(0x18, 0x5A, 0xBD)); // AppInfoBrush
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0x26, 0x31, 0x3B)); // AppMutedInkBrush

    static RegistrationStateToForegroundBrushConverter()
    {
        PendingBrush.Freeze();
        AcceptedBrush.Freeze();
        RejectedBrush.Freeze();
        CompletionBrush.Freeze();
        DefaultBrush.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            RegistrationState.Pending => PendingBrush,
            RegistrationState.Accepted => AcceptedBrush,
            RegistrationState.Rejected => RejectedBrush,
            RegistrationState.CompletionRequested => CompletionBrush,
            _ => DefaultBrush
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class VisibilityToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            "public_summary" => "ملخص عام",
            "relationship_visible" => "تفاصيل كاملة (للمعلم)",
            "student_visible" => "مرئي للطالب",
            _ => value?.ToString() ?? "عام"
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

