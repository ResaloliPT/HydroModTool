namespace HydroToolChain.Blazor.Business.Abstracts;

public interface IAppLoaderService
{
    event Action LoadersChanged;
    
    bool HasLoaders { get; }
    
    void AddLoader(LoadersNames name);

    void RemoveLoader(LoadersNames name);
}