using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Halaqa.Desktop.Shared.Presentation.Controls;

/// <summary>
/// Dependency-free icon control backed by the Windows Segoe MDL2 Assets font.
/// The names mirror the screen markup and keep the desktop UI consistent.
/// </summary>
public sealed class AppIcon : TextBlock
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind),
        typeof(string),
        typeof(AppIcon),
        new PropertyMetadata(string.Empty, OnKindChanged));

    public string Kind
    {
        get => (string)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    private static void OnKindChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        var icon = (AppIcon)dependencyObject;
        icon.Text = GetGlyph(args.NewValue as string);
        icon.FontFamily = new FontFamily("Segoe MDL2 Assets");
        icon.FontSize = 18;
        icon.TextAlignment = TextAlignment.Center;
        icon.VerticalAlignment = VerticalAlignment.Center;
    }

    private static string GetGlyph(string? kind) => kind switch
    {
        "ArrowRight" => "\uE72A",
        "ChevronRight" => "\uE76C",
        "ChevronLeft" => "\uE76B",
        "Close" => "\uE711",
        "Login" => "\uE8AC",
        "Replay" => "\uE72C",
        "MicrophoneOutline" => "\uE720",
        "RecordCircleOutline" => "\uE7C8",
        "VideoOutline" => "\uE714",
        "AccountVideoOutline" => "\uE714",
        "LockCheck" or "LockReset" => "\uE72E",
        "InformationOutline" => "\uE946",
        "DeleteOutline" => "\uE74D",
        "CalendarCheckOutline" => "\uE787",
        "ChartLine" => "\uE9D2",
        "BookOpenPageVariantOutline" => "\uE82D",
        "BellOutline" => "\uEA8F",
        "FileCertificateOutline" => "\uE8A5",
        "AccountMultipleCheckOutline" or "AccountGroupOutline" => "\uE716",
        "AccountEditOutline" or "AccountBoxOutline" => "\uE77B",
        _ => "\uE10C"
    };
}
