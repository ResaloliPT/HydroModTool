using HydroToolChain.App.Configuration.Models;
using HydroToolChain.App.Models;

namespace HydroToolChain.App;

public class AppOptions
{
    public Func<string, string, IReadOnlyCollection<string>> OpenAssetsDialog { get; init; }
    public Func<string?> SelectConfigDialog { get; init; }
    public Func<ConfigPartials?, string?> SaveConfigDialog { get; init; }
    public Action<MessageType, string> ShowMessage { get; init; }

}