using System.Windows;

namespace Halaqa.Desktop.Presentation;

public partial class MainWindow : Window
{
    public MainWindow(MainShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
