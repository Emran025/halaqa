using System.Windows;
using System.Windows.Controls;

namespace Halaqa.Desktop.Shared.Presentation.Controls;

/// <summary>
/// A lightweight, dependency-free icon element for the desktop client.
/// It keeps the XAML icon names used by the application while rendering
/// universally available Unicode symbols.
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
        icon.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Symbol");
        icon.TextAlignment = TextAlignment.Center;
    }

    private static string GetGlyph(string? kind) => kind switch
    {
        "ArrowRight" or "ChevronRight" => "›",
        "ChevronLeft" => "‹",
        "Close" => "×",
        "Login" => "→",
        "Replay" => "↻",
        "MicrophoneOutline" => "●",
        "RecordCircleOutline" => "●",
        "VideoOutline" or "AccountVideoOutline" => "▶",
        "LockCheck" or "LockReset" => "▣",
        "InformationOutline" => "i",
        "DeleteOutline" => "×",
        "CalendarCheckOutline" => "✓",
        "ChartLine" => "⌁",
        "BookOpenPageVariantOutline" => "▤",
        "BellOutline" => "◉",
        "FileCertificateOutline" => "▧",
        "AccountMultipleCheckOutline" or "AccountGroupOutline" => "●●",
        "AccountEditOutline" or "AccountBoxOutline" => "●",
        _ => "•"
    };
}
