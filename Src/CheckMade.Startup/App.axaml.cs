using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace CheckMade.Startup;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Desktop.Views.MainWindow
            {
                DataContext = new Desktop.ViewModels.MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new Mobile.Views.MainView
            {
                DataContext = new Mobile.ViewModels.MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}