using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaWebView;
using HydroToolChain.Avalonia.ViewModels;
using HydroToolChain.Avalonia.Views;

namespace HydroToolChain.Avalonia;

public partial class App : Application
{
    public override void RegisterServices()
    {
        base.RegisterServices();
        
        // if you use only WebView  
        AvaloniaWebViewBuilder.Initialize(default);
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindowView
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}