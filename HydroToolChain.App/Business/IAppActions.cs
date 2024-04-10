using HydroToolChain.App.Configuration.Models;

namespace HydroToolChain.App.Business;

public interface IAppActions
{
    Task<bool> ImportConfig(CancellationToken ct);
    
    Task<bool> ExportConfig(ConfigPartials? partial, CancellationToken ct);

    Task<(bool success, string readme)> GetReadmeAsync(CancellationToken ct);
    
    void LaunchGame();
    
    bool ClearModsFolder();
    
    bool OpenModsFolder();
}