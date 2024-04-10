using HydroToolChain.Blazor.DTOs;

namespace HydroToolChain.Blazor.Business;

internal class AppContext
{
    public event Action<bool> OnAppLoaded = loaded => {};

    public event Action OnAppStateChanged = () => {};

    public event Action<ProjectData> OnProjectChanged = project => {};

    private bool _appLoading;
    public bool AppLoading
    {
        get => _appLoading;
        set
        {
            _appLoading = value;
            OnAppStateChanged.Invoke();
        }
    }
    
    public void SetLoaded(bool loaded)
    {
        if (loaded)
        {
            OnAppLoaded.Invoke(loaded);
        }
    }

    public void ProjectChanged(ProjectData projectId)
    {
        OnProjectChanged?.Invoke(projectId);
    }
    
    public void StateChanged()
    {
        OnAppStateChanged?.Invoke();
    }
}