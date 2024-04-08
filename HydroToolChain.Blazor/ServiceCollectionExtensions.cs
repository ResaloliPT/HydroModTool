using HydroToolChain.App.Configuration.Models;
using HydroToolChain.App.Models;
using HydroToolChain.App.Tools;
using HydroToolChain.App.WindowsHelpers;
using HydroToolChain.Blazor.Business;
using HydroToolChain.Blazor.Business.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace HydroToolChain.Blazor;

public static class ServiceCollectionExtensions
{
    public static void RegisterBlazorServices(
        this IServiceCollection services,
        Action<BlazorServiceOptions> configureBlazorOptions)
    {
        services.AddOptions<BlazorServiceOptions>().Configure(configureBlazorOptions);
        
        services.AddTransient(typeof(IAppFacade), typeof(AppFacade));
        services.AddSingleton<Business.AppContext>();
        services.AddSingleton<AppLoader>();
        
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration = new SnackbarConfiguration
            {
                PositionClass = "mud-snackbar-location-bottom-right",
                NewestOnTop = true,
                PreventDuplicates = true,
                MaxDisplayedSnackbars = 5
            };
        });
        services.AddMudMarkdownServices();

        services.AddBlazorContextMenu();
        
        services.ConfigureTools<IDialogService>((options, dialog) =>
        {
            options.ShowMessage = (message, type) =>
            {
                switch (type)
                {
                    case MessageType.Error:
                        dialog.ShowMessageBox("Error", message);
                        break;
                    case MessageType.Warning:
                        dialog.ShowMessageBox("Warning", message);
                        break;
                    case MessageType.Info:
                        dialog.ShowMessageBox("Info", message);
                        break;
                }
            };
        });
        
        services.ConfigureWindowsHelpers();
    }
    
    public sealed class BlazorServiceOptions
    {
        public Func<string, string, Task<IReadOnlyCollection<string>>> FilesHelper { get; set; } =
            (_, _) => Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());

        public Func<Task<string?>> ConfigImporterHelper { get; set; } = () => Task.FromResult<string?>(null);
        
        public Func<ConfigPartials?, string, Task<bool?>> ConfigExportHelper { get; set; } = (_, _) => Task.FromResult<bool?>(null);
    }   
}