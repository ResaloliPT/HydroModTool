using HydroToolChain.App.Models;

namespace HydroToolChain.Blazor;

public class BlazorOptions
{
    public Func<string, string?> SelectFolder { get; init; }
    public Action<MessageType, string> ShowFormMessage { get; init; }
}