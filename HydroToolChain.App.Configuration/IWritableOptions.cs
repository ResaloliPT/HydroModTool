using Microsoft.Extensions.Options;

namespace HydroToolChain.App.Configuration;

public interface IWritableOptions<out T> : IOptions<T> where T : class, new()
{
    void RequestUpdate();

    T GetValue();
    
    Task Update(Action<T> applyChanges);
    Task Update(Action<T> applyChanges, CancellationToken ct);
}  