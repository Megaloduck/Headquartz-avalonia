using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Headquartz.App.Views;
using Headquartz.App.Services;
using Headquartz.Simulation.Modules.Base;

namespace Headquartz.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ThemeService.Instance.Initialize();
        IndustryModuleRegistry.Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is
            IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
    
        base.OnFrameworkInitializationCompleted();
    }
}