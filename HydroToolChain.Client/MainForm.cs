using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace HydroToolChain.Client;

public partial class MainForm : Form
{
    public MainForm(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        
        blazorWebView1.RootComponents.Add<Blazor.Components.App>("#app");
        blazorWebView1.Services = serviceProvider;
        blazorWebView1.HostPage = "wwwroot\\index.html";
        blazorWebView1.BlazorWebViewInitialized += async (_, _) =>
        {
            await Task.Delay(1000);
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            Show();
            Activate();
            Focus();
        };
    }
}