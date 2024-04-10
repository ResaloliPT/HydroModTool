using HydroToolChain.Blazor.Business.Abstracts;

namespace HydroToolChain.Blazor.Business;

internal class AppLoaderService : IAppLoaderService
{
    public event Action? LoadersChanged;
    
    private ISet<LoadersNames> Loaders { get; } = new HashSet<LoadersNames>(){ LoadersNames.AppStarting };

    public bool HasLoaders => Loaders.Count > 0;
    
    public void AddLoader(LoadersNames name)
    {
        Loaders.Add(name);
        LoadersChanged?.Invoke();
    }

    public void RemoveLoader(LoadersNames name)
    {
        Loaders.Remove(name);
        LoadersChanged?.Invoke();
    }
}