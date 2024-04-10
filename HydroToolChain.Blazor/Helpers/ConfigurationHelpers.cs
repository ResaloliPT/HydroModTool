using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.Models;
using HydroToolChain.Blazor.State;
using Microsoft.Extensions.Options;

namespace HydroToolChain.Blazor.Helpers;

public class ConfigurationHelpers
{
    private readonly AppState _state;
    private readonly BlazorOptions _dialogs;
    private readonly IOptionsMonitor<AppData> _appData;

    public ConfigurationHelpers(
        IOptionsMonitor<AppData> appData,
        AppState state,
        BlazorOptions dialogs)
    {
        _state = state;
        _dialogs = dialogs;
        _appData = appData;
    }

    public void UpdateProjectsState()
    {
        var appData = _appData.CurrentValue;
        
        var projects = appData.Projects
                .Select(p => new ProjectData(p))
                .ToArray();

        var selectedProjectExists =
            appData.CurrentProject != Guid.Empty &&
            projects.Any(p => p.Id == appData.CurrentProject);

        Dictionary<Guid, IReadOnlyCollection<ProjectItemState>> projectItems;
        
        try
        {
            projectItems = appData.Projects
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
            _dialogs.ShowFormMessage(MessageType.Error, "Error loading config, make sure each project id is unique!");
            return;
        }

        _state.SelectedProject = selectedProjectExists
            ? appData.CurrentProject
            : projects
                .FirstOrDefault()?
                .Id ?? Guid.Empty;

        _state.Projects = projects.Select(p => new ProjectState
        {
            Id = p.Id,
            Name = p.Name,
            ModIndex = p.ModIndex,
            CookedAssetsPath = p.CookedAssetsPath,
            OutputPath = p.OutputPath,
        }).ToArray();

        _state.ProjectsItems = projectItems;
    }
}