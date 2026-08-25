using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Halaqa.Desktop.Shared.Presentation.State;

namespace Halaqa.Desktop.Shared.Presentation.Controls;

public partial class StatusBadge : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(StatusBadge),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ToneProperty = DependencyProperty.Register(
        nameof(Tone),
        typeof(StatusTone),
        typeof(StatusBadge),
        new PropertyMetadata(StatusTone.Neutral, OnToneChanged));

    public StatusBadge()
    {
        InitializeComponent();
        ApplyTone(Tone);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public StatusTone Tone
    {
        get => (StatusTone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    private static void OnToneChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
        if (dependencyObject is StatusBadge badge)
        {
            badge.ApplyTone((StatusTone)eventArgs.NewValue);
        }
    }

    private void ApplyTone(StatusTone tone)
    {
        if (BadgeBorder is null || BadgeText is null)
        {
            return;
        }

        var (backgroundKey, foregroundKey) = tone switch
        {
            StatusTone.Info => ("AppInfoSoftBrush", "AppInfoBrush"),
            StatusTone.Success => ("AppSuccessSoftBrush", "AppSuccessBrush"),
            StatusTone.Warning => ("AppWarningSoftBrush", "AppWarningBrush"),
            StatusTone.Error => ("AppErrorSoftBrush", "AppErrorBrush"),
            _ => ("AppSurfaceMutedBrush", "AppMutedInkBrush")
        };
        BadgeBorder.Background = (Brush)FindResource(backgroundKey);
        BadgeText.Foreground = (Brush)FindResource(foregroundKey);
    }
}
