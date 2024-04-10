namespace HydroToolChain.Blazor.State;

public record GuidState
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public Guid RetailGuid { get; init; }
    public Guid ModdedGuid { get; init; }
}