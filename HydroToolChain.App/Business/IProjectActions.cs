using HydroToolChain.App.Configuration.Data;

namespace HydroToolChain.App.Business;

public interface IProjectActions
{
    Task<IReadOnlyCollection<ProjectData>> AddProject(ProjectData project, CancellationToken ct);
    Task DeleteProject(Guid projectId, CancellationToken ct);
    Task SetCurrentProject(Guid projectId, CancellationToken ct);
    bool OpenDistFolder(Guid projectId);
    Task UpdateProject(ProjectData projectData, CancellationToken ct);
}