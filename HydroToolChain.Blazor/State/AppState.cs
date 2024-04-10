namespace HydroToolChain.Blazor.State;

public record AppState
{
    public string Readme = string.Empty;
    public bool GotReadme = false;
    
    public Guid SelectedProject = Guid.Empty;
    
    public IReadOnlyCollection<ProjectState> Projects = new List<ProjectState>();

    public IDictionary<Guid, IReadOnlyCollection<ProjectItemState>> ProjectsItems =
        new Dictionary<Guid, IReadOnlyCollection<ProjectItemState>>();
    
    public IReadOnlyCollection<GuidState> GUIDs = new List<GuidState>();
    
    public IReadOnlyCollection<UidState> UIDs = new List<UidState>();
}