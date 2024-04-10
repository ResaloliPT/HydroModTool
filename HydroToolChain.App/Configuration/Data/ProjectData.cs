namespace HydroToolChain.App.Configuration.Data;

public class ProjectData
{
    public ProjectData(ProjectData copy)
    {
        Id = Guid.NewGuid();
        Name = copy.Name;
        ModIndex = copy.ModIndex;
        CookedAssetsPath = copy.CookedAssetsPath;
        OutputPath = copy.OutputPath;
        Items = copy.Items;
    }
    
    public ProjectData(Guid id)
    {
        Id = id;
        Items = new List<ProjectItemData>();
    }

    public Guid Id { get; set; } = Guid.Empty;
    public List<ProjectItemData> Items { get; set; } = new List<ProjectItemData>();
    public string Name { get; set; } = string.Empty;
    public int ModIndex { get; set; } = 500;
    public string CookedAssetsPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
}