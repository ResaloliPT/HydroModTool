using HydroToolChain.App.Models;
using MudBlazor;

namespace HydroToolChain.Blazor.Helpers;

internal class Helpers
{
    private readonly ISnackbar _snackbar;

    public Helpers(
        ISnackbar snackbar)
    {
        _snackbar = snackbar;
    }
    
    public void ShowToast(string message, MessageType type)
    {
        switch (type)
        {
            case MessageType.Error:
                _snackbar.Add(message, Severity.Error, key: Guid.NewGuid().ToString("N"));

                break;
            case MessageType.Warning:
                _snackbar.Add(message, Severity.Warning, key: Guid.NewGuid().ToString("N"));
                
                break;
            case MessageType.Info:
                _snackbar.Add(message, Severity.Info, key: Guid.NewGuid().ToString("N"));
                
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}