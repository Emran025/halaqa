using System.Windows.Controls;

namespace Halaqa.Desktop.Features.Quran.Presentation.Views;

public partial class QuranReaderView : UserControl
{
    public QuranReaderView()
    {
        InitializeComponent();
        Loaded += (_, _) => Focus();
        MouseDown += (_, _) => Focus();
    }
}
