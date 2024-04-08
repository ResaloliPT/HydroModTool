using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.UE4Pak.Paker;

namespace HydroToolChain.App.Tools;

public class ToolsService : IToolsService
{
    private readonly Stager _stager;
    private readonly IPaker _paker;
    private readonly ProjectTools _projectTools;

    public ToolsService(
        Stager stager,
        IPaker paker,
        ProjectTools projectTools)
    {
        _stager = stager;
        _paker = paker;
        _projectTools = projectTools;
    }
    
    public Task PackageAsync(ProjectData project)
    {
        return _paker.PakMod(project.OutputPath, project.Name, project.ModIndex, CancellationToken.None);
    }

    public Task StageAsync(ProjectData project, IReadOnlyCollection<GuidData> guids, IReadOnlyCollection<UidData> uids)
    {
        return _stager.StageAsync(project, guids, uids);
    }

    public Task CopyFiles(ProjectData project)
    {
        return _projectTools.CopyFiles(project);
    }
}