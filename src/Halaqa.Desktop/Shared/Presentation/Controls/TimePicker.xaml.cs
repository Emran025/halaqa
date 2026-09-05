using System.Windows;
using System.Windows.Controls;

namespace Halaqa.Desktop.Shared.Presentation.Controls;

public partial class TimePicker : UserControl
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(string),
        typeof(TimePicker),
        new FrameworkPropertyMetadata("18:00", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public TimePicker()
    {
        Times = Enumerable.Range(0, 48)
            .Select(index => TimeOnly.MinValue.AddMinutes(index * 30).ToString("HH:mm"))
            .ToArray();
        InitializeComponent();
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IReadOnlyList<string> Times { get; }

    private void OpenPicker(object sender, RoutedEventArgs e)
    {
        PickerPopup.IsOpen = true;
    }

    private void SelectTime(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
        {
            return;
        }

        Value = (string)e.AddedItems[0];
        PickerPopup.IsOpen = false;
    }
}
