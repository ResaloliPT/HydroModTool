namespace HydroToolChain.Blazor.State;

public record ProjectState
{
    public ProjectState(ProjectState copy)
    {
        Id = Guid.NewGuid();
        Name = copy.Name;
        ModIndex = copy.ModIndex;
        CookedAssetsPath = copy.CookedAssetsPath;
        OutputPath = copy.OutputPath;
    }
    
    public Guid Id { get; init; }
    public string Name { get; init; }
    public int ModIndex { get; init; }
    public string CookedAssetsPath { get; init; }
    public string OutputPath { get; init; }
}