using HydroToolChain.App.Business;
using HydroToolChain.App.Configuration;
using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.Models;
using HydroToolChain.Blazor;
using HydroToolChain.Blazor.Business;
using HydroToolChain.Blazor.Business.Abstracts;
using HydroToolChain.Blazor.Components;
using HydroToolChain.Blazor.Helpers;
using HydroToolChain.Blazor.Mappings;
using HydroToolChain.Blazor.State;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void RegisterBlazorServices(
        this IServiceCollection services,
        Func<IServiceProvider, BlazorOptions> configureOptions)
    {
#if DEBUG
        services.AddSassCompilerCore();
#endif

        services.AddSingleton<Helpers>();

        #region ComponentModels

        services.AddTransient<AppHeaderModel>();

        #endregion
        
        services.AddSingleton(configureOptions);
        
        services.AddSingleton<AppState>(srvc =>
        {
            var appData = srvc.GetRequiredService<IWritableOptions<AppData>>().GetValue();
            var appConfiguration = srvc.GetRequiredService<IAppConfiguration>();
            var appActions = srvc.GetRequiredService<IAppActions>();
            var dialogs = srvc.GetRequiredService<BlazorOptions>();
            
            var readMe = Task.Run(async () =>
                await appActions.GetReadmeAsync(CancellationToken.None)).GetAwaiter().GetResult();

            var projects = appData.Projects
                .Select(p => new ProjectData(p))
                .ToArray();

            var selectedProjectExists =
                appData.CurrentProject != Guid.Empty &&
                projects.Any(p => p.Id == appData.CurrentProject);

            Dictionary<Guid, IReadOnlyCollection<ProjectItemState>> projectItems;
            
            try
            {
                projectItems = projects
                    .ToDictionary(
                        p => p.Id,
                        p => (IReadOnlyCollection<ProjectItemState>)p.Items
                            .Select(i => new ProjectItemState
                            {
                                Id = i.Id,
                                Name = i.Name,
                                Path = i.Path
                            })
                            .ToArray());
            }
            catch (ArgumentException)
            {
                dialogs.ShowFormMessage(MessageType.Error, "Error loading config, make sure each project id is unique!");
                
                return new AppState();
            }
            
            var state = new AppState
            {
                Readme = Markdig.Markdown.ToHtml(readMe.readme),
                GotReadme = readMe.success,
                SelectedProject = selectedProjectExists ?
                    appData.CurrentProject :
                    projects
                        .FirstOrDefault()?
                        .Id ?? Guid.Empty,
                Projects = projects.Select(p => new ProjectState
                {
                    Id = p.Id,
                    Name = p.Name,
                    ModIndex = p.ModIndex,
                    CookedAssetsPath = p.CookedAssetsPath,
                    OutputPath = p.OutputPath,
                }).ToArray(),
                ProjectsItems = projectItems,
                GUIDs = appData.Guids
                    .Select(guid => new GuidState
                    {
                        Id = guid.Id,
                        Name = guid.Name,
                        RetailGuid = guid.RetailGuid,
                        ModdedGuid = guid.ModdedGuid
                    })
                    .ToArray(),
                UIDs = appData.Uids
                    .Select(uid => new UidState()
                    {
                        Id = uid.Id,
                        Name = uid.Name,
                        RetailUid = uid.RetailUid,
                        ModdedUid = uid.ModdedUid
                    })
                    .ToArray()
            };

            appConfiguration.ReloadAppData(state.ToAppData()!, CancellationToken.None);
            
            return state;
        });
        
        services.AddSingleton<IAppLoaderService, AppLoaderService>();
        services.AddTransient<IAppFacade, AppFacade>();
        services.AddSingleton<StateWatchers>();
        services.AddTransient<ConfigurationHelpers>();
        
        
        services.AddSingleton<HydroToolChain.Blazor.Business.AppContext>();
        
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
    }
}