using HydroToolChain.App;
using HydroToolChain.App.Models;
using HydroToolChain.Blazor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HydroToolChain.Client;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        var services = new ServiceCollection();
        var configurationBuilder = new ConfigurationBuilder();
        
        ConfigureServices(services, configurationBuilder);

        var configuration = configurationBuilder.Build();
        
        services.AddSingleton(configuration);
        
        var serviceProvider = services.BuildServiceProvider();
        
        Application.Run(new MainForm(serviceProvider));
    }
    
    private static void ConfigureServices(IServiceCollection services, IConfigurationBuilder configurationBuilder)
    {
        services.AddAppServices(svc =>
        {
            var dialogs = svc.GetRequiredService<Dialogs>();

            return new AppOptions
            {
                OpenAssetsDialog = dialogs.OpenAssetsDialog,
                SelectConfigDialog = dialogs.SelectConfigDialog,
                SaveConfigDialog = dialogs.SaveConfigDialog,
                ShowMessage = dialogs.ShowMessage, 
            };
        });
        
        services.AddSingleton<Dialogs>();
        
        #region Blazor
        services.AddWindowsFormsBlazorWebView();
        services.RegisterBlazorServices(services =>
        {
            var dialogs = services.GetRequiredService<Dialogs>();
            
            return new BlazorOptions
            {
                SelectFolder = dialogs.SelectFolderDialog,
                ShowFormMessage = ((type, message) =>
                {
                    var typeText = type switch
                    {
                        MessageType.Error => "Error",
                        MessageType.Warning => "Warning",
                        MessageType.Info => "Info",
                        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid message type")
                    };

                    var messageIcon = type switch
                    {
                        MessageType.Error => MessageBoxIcon.Error,
                        MessageType.Warning => MessageBoxIcon.Warning,
                        MessageType.Info => MessageBoxIcon.Information,
                        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid message type")
                    };

                    MessageBox.Show(message, typeText, MessageBoxButtons.OK, messageIcon);
                })
            };
        });
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
        #endregion
        
    }

}