using System.Diagnostics;
using System.Reactive;
using HydroToolChain.Avalonia.Models;
using ReactiveUI;

namespace HydroToolChain.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _isAppPageVisible = true;
    public bool IsAppPageVisible
    {
        get => _isAppPageVisible;
        set => this.RaiseAndSetIfChanged(ref _isAppPageVisible, value);
    }
    
    private bool _isAboutPageVisible = false;
    public bool IsAboutPageVisible
    {
        get => _isAboutPageVisible;
        set => this.RaiseAndSetIfChanged(ref _isAboutPageVisible, value);
    }
    
    public ReactiveCommand<PageType, Unit> TogglePage { get; }


    public MainWindowViewModel()
    {
        TogglePage = ReactiveCommand.Create<PageType>(type =>
        {
            IsAppPageVisible = type is PageType.App;
            IsAboutPageVisible = type is PageType.About;
        });
    }
    
    public void ShowGitHub()
    {
        Process.Start(new ProcessStartInfo()
        {
            UseShellExecute = true,
            FileName = "https://github.com/ResaloliPT/HydroModTool/"
        });
    }
}