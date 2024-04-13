using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform;
using AvaloniaWebView;
using HydroToolChain.Avalonia.ViewModels.Pages;
using Markdig;
using WebViewCore.Events;

namespace HydroToolChain.Avalonia.Views.Pages;

public partial class AboutPageView : UserControl, IDisposable
{
    private readonly WebView _browser;

    public AboutPageView()
    {
        DataContext = new AboutPageViewModel();
        InitializeComponent();

        _browser = this.Find<WebView>("webView")!;
        var fs = new StreamReader(AssetLoader.Open(new Uri("avares://HydroToolChain/Readme.md"), null));
        var readme = fs.ReadToEnd();
        fs.Close();
        
        fs = new StreamReader(AssetLoader.Open(new Uri("avares://HydroToolChain/Assets/Readme.html"), null));
        var html = fs.ReadToEnd();
        fs.Close();
        
        var markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseBootstrap();
        var htmlReadme = Markdown.ToHtml(readme, markdownPipeline.Build());

        html = html.Replace("{{markdown}}", htmlReadme);
        _browser.HtmlContent = html;
        
        _browser.NavigationStarting += BrowserOnNavigationStarting;
    }

    private void BrowserOnNavigationStarting(object? sender, WebViewUrlLoadingEventArg e)
    {
        Process.Start(new ProcessStartInfo()
        {
            UseShellExecute = true,
            FileName = e.Url!.ToString()
        });

        e.Cancel = true;
    }

    public void Dispose()
    {
        _browser.NavigationStarting -= BrowserOnNavigationStarting;
    }
}