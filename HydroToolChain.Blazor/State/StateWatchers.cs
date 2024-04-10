namespace HydroToolChain.Blazor.State;

public class StateWatchers
{
    public event Action OnProjectChanged = () => { };
    public event Action OnSelectedProjectChanged = () => { };

    public void TriggerOnProjectChanged()
    {
        OnProjectChanged.Invoke();
    }
    
    public void TriggerOnSelectedProjectChanged()
    {
        OnSelectedProjectChanged.Invoke();
    }
}