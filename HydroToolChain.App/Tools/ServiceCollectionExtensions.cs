using HydroToolChain.App.Models;
using HydroToolChain.App.UE4Pak;
using HydroToolChain.App.UE4Pak.Paker;
using Microsoft.Extensions.DependencyInjection;

namespace HydroToolChain.App.Tools;

public static class ServiceCollectionExtensions
{
    public static void ConfigureTools<TDep1> (  
        this IServiceCollection services, Action<ToolsServiceOptions, TDep1> configureOptions) where TDep1 : class
    {
        services.AddUE4Paker(UE4PakVersion.THREE);
        
        services.AddTransient<Stager>();
        services.AddTransient<ProjectTools>();
        
        services.AddOptions<ToolsServiceOptions>().Configure(configureOptions);
        services.AddTransient<IToolsService, ToolsService>();
    }
    
    public class ToolsServiceOptions
    {
        public Action<string, MessageType> ShowMessage { get; set; } = (_, _) => { };
    }
}