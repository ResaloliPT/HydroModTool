using HydroToolChain.App;
using HydroToolChain.App.Business;
using HydroToolChain.App.Configuration;
using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.Tools;
using HydroToolChain.App.UE4Pak;
using HydroToolChain.App.WindowsHelpers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(
        this IServiceCollection services,
        Func<IServiceProvider, AppOptions> configureAppOptions)
    {
        services.AddSingleton(configureAppOptions);

        services.AddSingleton<IAppActions, AppActions>();
        services.AddSingleton<IProjectActions, ProjectActions>();
        services.AddTransient<IWindowsHelpers, WindowsHelpers>();
        services.ConfigureWritable<AppData>("appData.json", false, AppConfiguration.FileRoot);
        
        services.AddUE4Paker(UE4PakVersion.THREE);
        
        services.AddTransient<Stager>();
        services.AddTransient<ProjectTools>();
        
        services.AddTransient<IToolsService, ToolsService>();

        services.AddTransient<IAppConfiguration, AppConfiguration>();
        
        return services;
    }
}