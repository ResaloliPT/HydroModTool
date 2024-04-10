using HydroToolChain.App;
using HydroToolChain.App.Business;
using HydroToolChain.App.Models;
using HydroToolChain.Blazor.Business.Abstracts;
using HydroToolChain.Blazor.Components.Dialogs;
using HydroToolChain.Blazor.Helpers;
using HydroToolChain.Blazor.Models;
using HydroToolChain.Blazor.State;
using MudBlazor;
using MudBlazor.Extensions;
using ProjectData = HydroToolChain.Blazor.DTOs.ProjectData;

namespace HydroToolChain.Blazor.Business;

internal class AppFacade : IAppFacade
{
    private readonly Helpers.Helpers _helpers;
    private readonly ConfigurationHelpers _configurationHelpers;
    private readonly StateWatchers _stateWatchers;
    private readonly IProjectActions _projectActions;
    private readonly IAppActions _appActions;
    private readonly AppState _appState;
    private readonly IDialogService _dialogService;
    private readonly ISnackbar _snackbar;
    private readonly IAppLoaderService _loaderService;

    public AppFacade(
        Helpers.Helpers helpers,
        ConfigurationHelpers configurationHelpers,
        StateWatchers stateWatchers,
        IProjectActions projectActions,
        IAppActions appActions,
        AppState appState,
        IDialogService dialogService,
        ISnackbar snackbar,
        IAppLoaderService loaderService)
    {
        _helpers = helpers;
        _configurationHelpers = configurationHelpers;
        _stateWatchers = stateWatchers;
        _projectActions = projectActions;
        _appActions = appActions;
        _appState = appState;
        _dialogService = dialogService;
        _snackbar = snackbar;
        _loaderService = loaderService;
    }
    
    public event Action? OnAppStateChanged;

    public async Task AddProject(CancellationToken ct)
    {
        var result = (await (await _dialogService.ShowAsync<AddProjectDialog>()!).Result)!.Data.As<ProjectData?>();

        if (result == null)
        {
            return;
        }

        _loaderService.AddLoader(LoadersNames.AddProject);
        
        var projects = await _projectActions.AddProject(new App.Configuration.Data.ProjectData(result.Id)
        {
            Name = result.Name,
            ModIndex = result.ModIndex,
            OutputPath = result.OutputPath,
            CookedAssetsPath = result.CookedAssetsPath
        }, ct);

        _appState.Projects = projects
            .Select(p => new ProjectState
            {
                Id = p.Id,
                Name = p.Name,
                ModIndex = p.ModIndex,
                OutputPath = p.OutputPath,
                CookedAssetsPath = p.CookedAssetsPath
            }).ToArray();
        
        _loaderService.RemoveLoader(LoadersNames.AddProject);
        
        _helpers.ShowToast($"New project [{result.Name}] added", MessageType.Info);
        
        _stateWatchers.TriggerOnProjectChanged();
    }

    public Task<bool> Stage(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Package(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Copy(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public void LaunchGame()
    {
        _appActions.LaunchGame();
    }

    public async Task DevExpress(CancellationToken ct)
    {
        if (await Stage(ct) && await Package(ct) && await Copy(ct))
        {
            LaunchGame();
        }
        else
        {
            _helpers.ShowToast("Failed to Dev Express, try again each step by themselves", MessageType.Error);
        }
    }

    public void OpenModsFolder()
    {
        if (!_appActions.OpenModsFolder())
        {
            _helpers.ShowToast($"Error opening asset folder [{Constants.PaksFolder}]", MessageType.Warning);
        }
    }

    public void ClearLegacyMods()
    {
        if (_appActions.ClearModsFolder())
        {
            _helpers.ShowToast("Legacy Mods Cleared.", MessageType.Info);
        }
        else
        {
            _helpers.ShowToast("Failed to clear mods folder. Check if game is running.", MessageType.Warning);
        }
    }

    public async Task SetCurrentProject(Guid projectId, CancellationToken ct)
    {
        _loaderService.AddLoader(LoadersNames.ChangeProject);
        
        await _projectActions.SetCurrentProject(projectId, ct);

        _appState.SelectedProject = projectId;
        
        _loaderService.RemoveLoader(LoadersNames.ChangeProject);
        
        _stateWatchers.TriggerOnSelectedProjectChanged();
    }

    public async Task EditProject(Guid projectId, CancellationToken ct)
    {
        var project = _appState.Projects
            .First(p => p.Id == projectId);
        
        var result = (await (await _dialogService.ShowAsync<EditProjectDialog>(null, new DialogParameters
        {
            {"Project", new ProjectData(project)}
        })!).Result)!.Data.As<ProjectData?>();

        _loaderService.AddLoader(LoadersNames.EditProject);
        
        if (result == null)
        {
            _loaderService.RemoveLoader(LoadersNames.EditProject);
            return;
        }

        await _projectActions.UpdateProject(new App.Configuration.Data.ProjectData(result.Id)
        {
            Name = result.Name,
            ModIndex = result.ModIndex,
            OutputPath = result.OutputPath,
            CookedAssetsPath = result.CookedAssetsPath
        }, ct);

        var existingProjects = _appState.Projects
            .ToList();

        var index = existingProjects
            .FindIndex(p => p.Id == projectId);

        existingProjects[index] = new ProjectState
        {
            Id = result.Id,
            Name = result.Name,
            ModIndex = result.ModIndex,
            OutputPath = result.OutputPath,
            CookedAssetsPath = result.CookedAssetsPath
        };

        _appState.Projects = existingProjects;
        
        _loaderService.RemoveLoader(LoadersNames.EditProject);
    }

    public async Task DeleteProject(Guid projectId, CancellationToken ct)
    {
        _loaderService.AddLoader(LoadersNames.DeleteProject);

        var project = _appState.Projects
            .First(p => p.Id == projectId);

        await _projectActions.DeleteProject(projectId, ct);

        _appState.Projects = _appState.Projects
            .Where(p => p.Id != projectId)
            .ToArray();

        var changedProject = false;
        if (_appState.SelectedProject == projectId)
        {
            _appState.SelectedProject = _appState.Projects
                .FirstOrDefault()?.Id ?? Guid.Empty;
            changedProject = true;
        }
        
        _loaderService.RemoveLoader(LoadersNames.DeleteProject);
        
        _helpers.ShowToast($"Project [{project.Name}] was removed", MessageType.Info);
        
        _stateWatchers.TriggerOnProjectChanged();
        if (changedProject)
        {
            _stateWatchers.TriggerOnSelectedProjectChanged();
        }
    }

    public void OpenDistFolder(Guid projectId)
    {
        if (!_projectActions.OpenDistFolder(projectId))
        {
            _helpers.ShowToast("Failed to open dist folder.", MessageType.Warning);
        }
    }

    public (ProjectState? project, IReadOnlyCollection<ProjectItemState> items) GetCurrentProject()
    {
        _appState.ProjectsItems.TryGetValue(_appState.SelectedProject, out var items);
        
        return (_appState.Projects
            .FirstOrDefault(p => p.Id == _appState.SelectedProject), items ?? Array.Empty<ProjectItemState>());
    }

    public IReadOnlyCollection<ProjectState> GetProjects()
    {
        return _appState.Projects;
    }

    public Task AddAssets(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAssets(IReadOnlyCollection<Guid> assetsToDelete, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public void AddUid()
    {
        throw new NotImplementedException();
    }

    public void RemoveUid(Guid? uidId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<UidState> GetUids()
    {
        return _appState.UIDs;
    }

    public void UpdateUid(UidState? uidData)
    {
        throw new NotImplementedException();
    }

    public void AddGuid()
    {
        throw new NotImplementedException();
    }

    public void RemoveGuid(Guid? guidId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<GuidState> GetGuids()
    {
        return _appState.GUIDs;
    }

    public void UpdateGuid(GuidState? guidData)
    {
        throw new NotImplementedException();
    }

    public string GetReadme()
    {
        return _appState.Readme;
    }
}