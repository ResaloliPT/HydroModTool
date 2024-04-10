using HydroToolChain.App.Configuration.Data;
using HydroToolChain.Blazor.Models.Data;
using HydroToolChain.Blazor.State;

namespace HydroToolChain.Blazor.Helpers;

internal static class TreeHelpers
{
    public static HashSet<ProjectAssetItem> GetTreeNodesFromProject(IReadOnlyCollection<ProjectItemState> projectAssets)
    {
        var resultTree = new HashSet<ProjectAssetItem>();

        var rootNode = new ProjectAssetItem("root");

        var orderedItems = projectAssets
            .OrderBy(i => i.Path);
        
        foreach (var item in orderedItems)
        {
            var pathParts = item.Path.Split("\\", StringSplitOptions.RemoveEmptyEntries);

            var lastTraversedNode = rootNode;

            for (var i = 0; i < pathParts.Length; i++)
            {
                var pathPart = pathParts[i];

                var partNode = lastTraversedNode.TreeItems.FirstOrDefault(ti => ti.DisplayName == pathPart);

                if (partNode == null && i == 0)
                {
                    var node = new ProjectAssetItem(pathPart);

                    lastTraversedNode = node;

                    rootNode.TreeItems.Add(node);
                    continue;
                }
                
                if (partNode == null && i == pathParts.Length - 1)
                {
                    lastTraversedNode.TreeItems.Add(new ProjectAssetItem(item));
                }
                else if (partNode == null)
                {
                    var node = new ProjectAssetItem(pathPart);

                    lastTraversedNode.TreeItems.Add(node);

                    lastTraversedNode = node;
                    continue;
                }
                    
                lastTraversedNode = partNode!;
            }
        }

        var contentFolderAsset = rootNode.TreeItems.Count > 0 ? rootNode.TreeItems.First() : null;

        if (contentFolderAsset == null)
        {
            return resultTree;
        }

        resultTree.Add(contentFolderAsset);

        return resultTree;
    }

    public static IReadOnlyCollection<Guid> GetAllChildAssetIds(ProjectAssetItem assetItem)
    {
        var childAssetIds = new List<Guid>();
        if (assetItem.Asset != null)
        {
            childAssetIds.Add(assetItem.Asset.Id);
        }

        foreach (var child in assetItem.TreeItems)
        {
            childAssetIds.AddRange(GetAllChildAssetIds(child));
        }

        return childAssetIds;
    }
}