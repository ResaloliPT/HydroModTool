using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.Configuration.Models;
using HydroToolChain.Blazor.State;

namespace HydroToolChain.Blazor.Mappings;

public static class AppStateMappings
{
    public static AppData? ToAppData(this AppState? state)
    {
        return state is null
            ? null
            : new AppData
            {
                Version = ConfigVersion._1,
                CurrentProject = state.SelectedProject,
                Guids = state.GUIDs.Select(guid => guid.ToAppData()!).ToList(),
                Uids = state.UIDs.Select(uid => uid.ToAppData()!).ToList(),
                Projects = state.Projects
                    .Select(p => p.ToAppdata(state)!)
                    .ToList()
            };
    }

    private static ProjectItemData? ToAppdata(this ProjectItemState? state)
    {
        return state is null
            ? null
            : new ProjectItemData
            {
                Id = state.Id,
                Name = state.Name,
                Path = state.Path
            }; 
    }
    
    private static ProjectData? ToAppdata(this ProjectState? projectState, AppState state)
    {
        return projectState is null
            ? null
            : new ProjectData(projectState.Id)
            {
                Name = projectState.Name,
                ModIndex = projectState.ModIndex,
                CookedAssetsPath = projectState.CookedAssetsPath,
                OutputPath = projectState.OutputPath,
                Items = state.ProjectsItems[projectState.Id]
                    .Select(i => i.ToAppdata()!)
                    .ToList()
            }; 
    }
    
    private static GuidData? ToAppData(this GuidState? state)
    {
        return state is null
            ? null
            : new GuidData
            {
                Id = state.Id,
                Name = state.Name,
                RetailGuid = state.RetailGuid,
                ModdedGuid = state.ModdedGuid
            };
    }
    
    private static UidData? ToAppData(this UidState? state)
    {
        return state is null
            ? null
            : new UidData
            {
                Id = state.Id,
                Name = state.Name,
                RetailUid = state.RetailUid,
                ModdedUid = state.ModdedUid
            };
    }
}