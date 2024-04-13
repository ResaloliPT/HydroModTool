using Avalonia.Controls;
using HydroToolChain.Avalonia.ViewModels;

namespace HydroToolChain.Avalonia.Views;

public partial class MainWindowView : Window
{
    public MainWindowView()
    {
        DataContext = new MainWindowViewModel();
        InitializeComponent();
    }
}