namespace HydroToolChain.Blazor.State;

public record ProjectItemState
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Path { get; init; }
}