namespace HydroToolChain.Blazor.Business;

internal class AppLoader
{
    public enum LoadersNames
    {
        TEST = 1,
        
    }
    
    private ISet<LoadersNames> Loaders { get; } = new HashSet<LoadersNames>();

    public bool HasLoaders => Loaders.Count > 0;
    
    public void AddLoader(LoadersNames name)
    {
        Loaders.Add(name);
    }

    public void RemoveLoader(LoadersNames name)
    {
        Loaders.Remove(name);
    }
}