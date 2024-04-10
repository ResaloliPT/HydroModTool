using HydroToolChain.App.Business;
using HydroToolChain.App.Models;
using HydroToolChain.Blazor.Business.Abstracts;
using HydroToolChain.Blazor.Components.Dialogs;
using HydroToolChain.Blazor.Helpers;
using HydroToolChain.Blazor.Models;
using HydroToolChain.Blazor.State;
using MudBlazor;
using MudBlazor.Extensions;

namespace HydroToolChain.Blazor.Components;

internal class AppHeaderModel
{
    private readonly ConfigurationHelpers _configurationHelpers;
    private readonly IAppActions _appActions;
    private readonly StateWatchers _stateWatchers;
    private readonly IDialogService _dialogService;
    private readonly IAppLoaderService _loaderService;
    private readonly Helpers.Helpers _helpers;

    public AppHeaderModel(
        ConfigurationHelpers configurationHelpers,
        IAppActions appActions,
        StateWatchers stateWatchers,
        IDialogService dialogService,
        IAppLoaderService loaderService,
        Helpers.Helpers helpers)
    {
        _configurationHelpers = configurationHelpers;
        _appActions = appActions;
        _stateWatchers = stateWatchers;
        _dialogService = dialogService;
        _loaderService = loaderService;
        _helpers = helpers;
    }
    
    public async Task LoadSettings(CancellationToken ct)
    {
        _loaderService.AddLoader(LoadersNames.LoadConfig);
        
        if (!await _appActions.ImportConfig(ct))
        {
            _loaderService.RemoveLoader(LoadersNames.LoadConfig);

            _helpers.ShowToast("Failed to import config", MessageType.Warning);
            return;
        }

        await Task.Delay(1000, ct);
        
        _helpers.ShowToast("Config imported", MessageType.Info);
        
        _loaderService.RemoveLoader(LoadersNames.LoadConfig);
        
        _configurationHelpers.UpdateProjectsState();
        
        _stateWatchers.TriggerOnProjectChanged();
        _stateWatchers.TriggerOnSelectedProjectChanged();
    }

    public async Task SaveSettings(CancellationToken ct)
    {
        var result = (await (await _dialogService.ShowAsync<SaveConfigDialog>()!).Result)!.Data.As<SaveConfigResult?>();

        if (result == null || !result.Save)
        {
            return;
        }
        
        _loaderService.AddLoader(LoadersNames.ExportConfig);
        
        if (!await _appActions.ExportConfig(result.PartialType, ct))
        {
            _loaderService.RemoveLoader(LoadersNames.ExportConfig);
            
            _helpers.ShowToast("Failed to export config", MessageType.Warning);
            return;
        }
        
        _loaderService.RemoveLoader(LoadersNames.ExportConfig);
        
        _helpers.ShowToast("Config exported", MessageType.Info);
    }
}