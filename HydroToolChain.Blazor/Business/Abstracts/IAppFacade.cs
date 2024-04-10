using HydroToolChain.Blazor.State;

namespace HydroToolChain.Blazor.Business.Abstracts;

public interface IAppFacade
{
    public event Action OnAppStateChanged;
    
    #region Project Main Actions
    
    Task AddProject(CancellationToken ct);

    Task<bool> Stage(CancellationToken ct);
    
    Task<bool> Package(CancellationToken ct);
    
    Task<bool> Copy(CancellationToken ct);

    void LaunchGame();

    Task DevExpress(CancellationToken ct);
    
    void OpenModsFolder();
    
    void ClearLegacyMods();
    
    #endregion

    #region Project Actions

    Task SetCurrentProject(Guid projectId, CancellationToken ct);
    
    Task EditProject(Guid projectId, CancellationToken ct);
    
    Task DeleteProject(Guid projectId, CancellationToken ct);
    
    void OpenDistFolder(Guid projectId);
    
    (ProjectState? project, IReadOnlyCollection<ProjectItemState> items) GetCurrentProject();
    
    IReadOnlyCollection<ProjectState> GetProjects();

    Task AddAssets(CancellationToken ct);
    
    Task RemoveAssets(IReadOnlyCollection<Guid> assetsToDelete, CancellationToken ct);
    #endregion

    #region UIDs Actions

    void AddUid();

    void RemoveUid(Guid? uidId);
    
    IReadOnlyCollection<UidState> GetUids();
    
    void UpdateUid(UidState? uidData);

    #endregion

    #region GUIDs Actions
    void AddGuid();

    void RemoveGuid(Guid? guidId);
    
    IReadOnlyCollection<GuidState> GetGuids();
    
    void UpdateGuid(GuidState? guidData);

    #endregion

    string GetReadme();
}