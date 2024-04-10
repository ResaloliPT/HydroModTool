namespace HydroToolChain.Blazor.State;

public record UidState
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string RetailUid { get; init; }
    public string ModdedUid { get; init; }
}