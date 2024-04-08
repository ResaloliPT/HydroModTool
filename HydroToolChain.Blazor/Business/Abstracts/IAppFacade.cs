using HydroToolChain.App.Configuration.Data;

namespace HydroToolChain.Blazor.Business.Abstracts;

public interface IAppFacade
{
    public event Action OnAppStateChanged;
    public event Action<Boolean> OnAppLoaded;

    public bool AppLoading { get; }
    
    #region AppActions

    Task LoadSettings();
    Task SaveSettings();
    
    #endregion
    
    #region Project Main Actions
    
    Task AddProject();

    Task<bool> Stage();
    
    Task<bool> Package();
    
    Task<bool> Copy();

    void LaunchGame();

    Task DevExpress();
    
    void OpenModsFolder();
    
    void ClearLegacyMods();
    
    #endregion

    #region Project Actions

    void SetCurrentProject(Guid projectId);
    
    Task EditProject(Guid projectId);
    
    void DeleteProject(Guid projectId);

    void OpenAssetsFolder(Guid projectId);
    
    void OpenDistFolder(Guid projectId);
    
    ProjectData? GetCurrentProject();
    
    IReadOnlyCollection<ProjectData> GetProjects();

    Task AddAssets();
    
    Task RemoveAssets(IReadOnlyCollection<Guid> assetsToDelete);
    #endregion

    #region UIDs Actions

    void AddUid();

    void RemoveUid(Guid? uidId);
    
    IReadOnlyCollection<UidData> GetUids();
    
    void UpdateUid(UidData? uidData);

    #endregion

    #region GUIDs Actions
    void AddGuid();

    void RemoveGuid(Guid? guidId);
    
    IReadOnlyCollection<GuidData> GetGuids();
    
    void UpdateGuid(GuidData? guidData);

    #endregion
}