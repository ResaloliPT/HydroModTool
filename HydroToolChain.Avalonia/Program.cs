using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Avalonia.WebView.Desktop;

namespace HydroToolChain.Avalonia;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .UseDesktopWebView()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}