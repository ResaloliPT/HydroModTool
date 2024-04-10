using HydroToolChain.Blazor.State;

namespace HydroToolChain.Blazor.Models.Data;

public class ProjectAssetItem
{
    public ProjectAssetItem (
        ProjectItemState asset)
    {
        Asset = asset;
    }
    
    public ProjectAssetItem (
        string folderName)
    {
        _folderName = folderName;
    }

    public Guid Identifier { get; } = Guid.NewGuid();
    
    public ProjectItemState? Asset { get; }

    public bool IsFolder => Asset == null;

    public HashSet<ProjectAssetItem> TreeItems { get; } = new();
        
    public bool IsExpanded { get; set; } = true;

    public bool HasChild => TreeItems.Count > 0;

    public string DisplayName => IsFolder ? _folderName! : Asset!.Name;

    private readonly string? _folderName;
}