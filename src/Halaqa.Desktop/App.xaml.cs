using System.Windows;
using Halaqa.Desktop.Config;
using Halaqa.Desktop.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace Halaqa.Desktop;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _services = Bootstrapper.BuildServices();
        var window = _services.GetRequiredService<MainWindow>();
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
