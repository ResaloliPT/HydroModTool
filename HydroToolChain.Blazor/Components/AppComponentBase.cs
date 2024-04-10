using MudBlazor;

namespace HydroToolChain.Blazor.Components;

public abstract class AppComponentBase : MudComponentBase, IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    protected CancellationToken CancellationToken => _cts.Token;

    protected Task UpdateView()
    {
        return InvokeAsync(StateHasChanged);
    }

    public virtual void DisposeComponent() { }
    
    public void Dispose()
    {
        DisposeComponent();
        _cts.Cancel();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}