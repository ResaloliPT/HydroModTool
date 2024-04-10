using HydroToolChain.App.Configuration;
using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.WindowsHelpers;

namespace HydroToolChain.App.Business;

internal class ProjectActions : IProjectActions
{
    private readonly IWritableOptions<AppData> _appData;
    private readonly IWindowsHelpers _windowsHelpers;

    public ProjectActions(
        IWritableOptions<AppData> appData,
        IWindowsHelpers windowsHelpers)
    {
        _appData = appData;
        _windowsHelpers = windowsHelpers;
    }
    
    public async Task<IReadOnlyCollection<ProjectData>> AddProject(ProjectData project, CancellationToken ct)
    {
        await _appData.Update(data =>
        {
            data.Projects.Add(project);
        }, ct);

        return _appData.GetValue().Projects;
    }

    public async Task DeleteProject(Guid projectId, CancellationToken ct)
    {
        await _appData.Update(appData =>
        {
            appData.Projects = appData.Projects
                .Where(p => p.Id != projectId)
                .ToList();

            if (appData.CurrentProject != projectId) return;
            
            appData.CurrentProject = appData.Projects
                .FirstOrDefault()?.Id ?? Guid.Empty;
        }, ct);
    }
    
    public async Task SetCurrentProject(Guid projectId, CancellationToken ct)
    {
        await _appData.Update(data =>
        {
            data.CurrentProject = projectId;
        }, ct);
    }

    public bool OpenDistFolder(Guid projectId)
    {
        var path = _appData.Value.Projects
            .First(p => p.Id == projectId)
            .OutputPath;

        try
        {
            _windowsHelpers.OpenFolder(path);
        }
        catch (Exception)
        {
            return false;
        }


        return true;
    }

    public async Task UpdateProject(ProjectData projectData, CancellationToken ct)
    {
        await _appData.Update(options =>
        {
            var projectToUpdate = options.Projects
                .Find(p => p.Id == projectData.Id)!;

            projectToUpdate.OutputPath = projectData.OutputPath;
            projectToUpdate.CookedAssetsPath = projectData.CookedAssetsPath;
            projectToUpdate.ModIndex = projectData.ModIndex;
            projectToUpdate.Name = projectData.Name;
        }, ct);
    }
}