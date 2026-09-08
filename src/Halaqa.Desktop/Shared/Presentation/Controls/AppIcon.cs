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
    private static readonly FontFamily IconFontFamily = new("Segoe MDL2 Assets, Segoe UI Emoji, Segoe UI Symbol");

    static AppIcon()
    {
        FontFamilyProperty.OverrideMetadata(
            typeof(AppIcon),
            new FrameworkPropertyMetadata(IconFontFamily, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
    }

    public AppIcon()
    {
        SetValue(FontFamilyProperty, IconFontFamily);
        TextAlignment = TextAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
    }

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
        icon.SetValue(FontFamilyProperty, IconFontFamily);
    }

    private static string GetGlyph(string? kind) => kind switch
    {
        "ArrowRight" => "\uE72A",
        "ChevronRight" => "\uE76C",
        "ChevronLeft" => "\uE76B",
        "Close" or "Cancel" => "\uE711",
        "Login" => "\uE8AC",
        "Logout" or "SignOut" => "\uF3B1",
        "Replay" or "Refresh" => "\uE72C",
        "MicrophoneOutline" => "\uE720",
        "RecordCircleOutline" => "\uE7C8",
        "VideoOutline" or "AccountVideoOutline" => "\uE714",
        "LockCheck" or "LockReset" => "\uE72E",
        "InformationOutline" => "\uE946",
        "DeleteOutline" => "\uE74D",
        "CalendarCheckOutline" or "CalendarClockOutline" => "\uE787",
        "ChartLine" => "\uE9D2",
        "BookOpenPageVariantOutline" or "BookOpenOutline" => "\uE82D",
        "BellOutline" => "\uEA8F",
        "FileCertificateOutline" => "\uE8A5",
        "AccountMultipleCheckOutline" or "AccountGroupOutline" or "AccountMultipleOutline" or "AccountMultiple" => "\uE716",
        "AccountEditOutline" or "AccountBoxOutline" or "AccountOutline" or "Account" or "AccountTie" or "AccountTieOutline" => "\uE77B",
        "AccountSearchOutline" or "AccountSearch" => "\uE721",
        "AccountClockOutline" or "ClockTimeFourOutline" or "Clock" => "\uE823",
        "CardAccountDetailsOutline" or "AccountCardDetailsOutline" => "\uE8D7",
        "EmailOutline" or "Email" or "Mail" => "\uE715",
        "PhoneOutline" or "Phone" => "\uE717",
        "MapMarkerOutline" or "MapMarker" => "\uE707",
        "SendOutline" or "Send" => "\uE724",
        "AlertCircleOutline" or "Alert" => "\uE7BA",
        _ => "\uE10C"
    };
}
