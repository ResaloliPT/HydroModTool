using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.Configuration.Models;

namespace HydroToolChain.App.Configuration;

public interface IAppConfiguration
{
    public Task<(bool success, string content)> ExportConfig(ConfigPartials? partial, CancellationToken ct);

    public Task<bool> TryImport(string filePath);
    
    public Task ReloadAppData(AppData data, CancellationToken ct);

}