using HydroToolChain.App.Configuration;
using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.Models;
using HydroToolChain.App.Tools;
using HydroToolChain.App.WindowsHelpers;
using HydroToolChain.Blazor.Components.Dialogs;
using HydroToolChain.Blazor.Models;
using MudBlazor;
using MudBlazor.Extensions;
using ProjectData = HydroToolChain.Blazor.DTOs.ProjectData;

namespace HydroToolChain.Blazor.Business;

internal sealed class AppFacadeOld// : IAppFacade
{
    private readonly IWritableOptions<AppData> _appDataOptions;
    private readonly IDialogService _dialogService;
    private readonly ISnackbar _snackbar;
    private readonly IToolsService _toolsService;
    private readonly IWindowsHelpers _windowsHelpers;
    private readonly IAppConfiguration _appConfiguration;

    private readonly AppContext _context;

    /*public AppFacadeOld(
        IWritableOptions<AppData> appDataOptions,
        IDialogService dialogService,
        ISnackbar snackbar,
        IToolsService toolsService,
        IWindowsHelpers windowsHelpers,
        IAppConfiguration appConfiguration,
        AppContext context)
    {
        _appDataOptions = appDataOptions;
        _dialogService = dialogService;
        _snackbar = snackbar;
        _toolsService = toolsService;
        _windowsHelpers = windowsHelpers;
        _appConfiguration = appConfiguration;
        _context = context;

        _appDataOptions.Update(options =>
        {
            var existingCurrentProject = options.CurrentProject;
            if (!options.Projects.Exists(p => p.Id == existingCurrentProject))
                options.CurrentProject = options.Projects
                    .FirstOrDefault()?.Id ?? Guid.Empty;
            
            
            _context.SetLoaded(true);
        });

    }
    
    public event Action OnAppStateChanged
    {
        add => _context.OnAppStateChanged += value;
        remove => _context.OnAppStateChanged -= value;
    }

    public event Action<bool> OnAppLoaded
    {
        add => _context.OnAppLoaded += value;
        remove => _context.OnAppLoaded -= value;
    }

    public async Task LoadSettings()
    {
        // TODO: Fix this
        string selectedFilePath = null;// await _blazorOptions.Value.ConfigImporterHelper();

        if (selectedFilePath == null)
        {
            return;
        }

        if (await _appConfiguration.TryImport(selectedFilePath))
        {
            _context.StateChanged();
            
            ShowToast("Config imported", MessageType.Info);
            return;
        }

        ShowToast("Failed to import config", MessageType.Warning);
    }

    public async Task SaveSettings()
    {
        var saveResult = await SaveSettingsInner();

        if (saveResult.HasValue && saveResult.Value)
        {
            ShowToast("Config exported", MessageType.Info);
        }
        else if (saveResult.HasValue && saveResult.Value == false)
        {
            ShowToast("Failed to export config", MessageType.Warning);
        }
    }

    public async Task AddProject()
    {
        var result = (await (await _dialogService.ShowAsync<AddProjectDialog>()!).Result)!.Data.As<ProjectData?>();

        if (result == null)
        {
            return;
        }
            
        await _appDataOptions.Update(options =>
        {
            options.CurrentProject = result.Id;
            options.Projects.Add(result);
        });
    }

    public void SetCurrentProject(Guid projectId)
    {
        _appDataOptions.Update(options =>
        {
            options.CurrentProject = projectId;
        }).ContinueWith(_ =>
        {
            _context.ProjectChanged(GetCurrentProject()!);
        });
    }

    public async Task EditProject(Guid projectId)
    {
        var project = GetProjectById(projectId);
        
        var result = (await (await _dialogService.ShowAsync<EditProjectDialog>(null, new DialogParameters
        {
            {"Project", project}
        })!).Result)!.Data.As<ProjectData?>();

        if (result == null)
        {
            return;
        }
        
        await _appDataOptions.Update(options =>
        {
            var projectToUpdate = options.Projects
                .Find(p => p.Id == projectId)!;

            projectToUpdate.OutputPath = result.OutputPath;
            projectToUpdate.CookedAssetsPath = result.CookedAssetsPath;
            projectToUpdate.ModIndex = result.ModIndex;
            projectToUpdate.Name = result.Name;
        });
    }

    public void DeleteProject(Guid projectId)
    {
        _appDataOptions.Update(appData =>
        {
            appData.Projects = appData.Projects
                .Where(p => p.Id != projectId)
                .ToList();

            if (appData.CurrentProject != projectId) return;
            
            appData.CurrentProject = appData.Projects
                .FirstOrDefault()?.Id ?? Guid.Empty;
            
            _context.ProjectChanged(GetCurrentProject()!);
        });
    }
    
    public ProjectData? GetCurrentProject()
    {
        var data = _appDataOptions.Value;

        return data.Projects
            .FirstOrDefault(p => p.Id == data.CurrentProject);
    }

    public ProjectData GetProjectById(Guid projectId)
    {
        return _appDataOptions.Value.Projects
            .First(p => p.Id == projectId);
    }

    public IReadOnlyCollection<ProjectData> GetProjects()
    {
        return _appDataOptions.Value.Projects;
    }

    public async Task AddAssets()
    {
        var currentProject = GetCurrentProject()!;

        var contentPath = Path.Combine(currentProject.CookedAssetsPath, "Content");

        //TODO: Fix this
        var selectedFiles = Array.Empty<string>();//await _blazorOptions.Value.FilesHelper("Select assets", contentPath);

        try
        {
            await _appDataOptions.Update(options =>
            {
                var projectToUpdate = options.Projects
                    .First(p => p.Id == options.CurrentProject);

                var newAssets = selectedFiles
                    .Select(path =>
                    {
                        var file = new FileInfo(path);

                        var assetPath = file.FullName.Replace(currentProject.CookedAssetsPath, "");

                        if (!assetPath.StartsWith("\\Content"))
                        {
                            throw new FormatException();
                        }

                        if (projectToUpdate.Items.Any(i => i.Path == assetPath))
                        {
                            return null;
                        }

                        return new ProjectItemData
                        {
                            Name = file.Name,
                            Path = assetPath
                        };
                    })
                    .Where(i => i != null)
                    .Select(i => i!)
                    .ToList();

                projectToUpdate.Items
                    .AddRange(newAssets);

                if (newAssets.Count < selectedFiles.Length && newAssets.Count > 0)
                {
                    ShowToast("Added new Assets, but some where already imported", MessageType.Info);
                    return;
                }
                
                if (newAssets.Count == 0)
                {
                    ShowToast("All assets already imported or no selected assets", MessageType.Info);
                    return;
                }

                ShowToast("Added new Assets", MessageType.Info);
            });
        }
        catch (FormatException)
        {
            ShowToast("Some files where Invalid, make sure you select files from the cooked folder", MessageType.Error);
        }
    }

    public async Task RemoveAssets(IReadOnlyCollection<Guid> assetsToDelete)
    {
        await _appDataOptions.Update(options =>
        {
            var projectToUpdate = options.Projects
                .First(p => p.Id == options.CurrentProject);
            
            projectToUpdate.Items = 
                projectToUpdate.Items
                    .Where(i => !assetsToDelete.Contains(i.Id))
                    .ToList();
        });
    }

    public async Task<bool> Stage()
    {
        var project = GetCurrentProject();

        if (project == null)
        {
            ShowToast("No Selected Project", MessageType.Warning);
            return false;
        }
        
        var data = _appDataOptions.Value;

        await _toolsService.StageAsync(project, data.Guids, data.Uids);
        
        ShowToast($"Mod [{project.Name}] Staged", MessageType.Info);

        return true;
    }

    public async Task<bool> Package()
    {
        var project = GetCurrentProject();

        if (project == null)
        {
            ShowToast("No Selected Project", MessageType.Warning);
            return false;
        }

        await _toolsService.PackageAsync(project);
        
        
        ShowToast($"Mod [{project.Name}] Packaged", MessageType.Info);

        return true;
    }

    public async Task<bool> Copy()
    {
        var project = GetCurrentProject();

        if (project == null)
        {
            ShowToast("No Selected Project", MessageType.Warning);
            return false;
        }

        await _toolsService.CopyFiles(project);

        ShowToast($"Mod [{project.Name}] Copied", MessageType.Info);
        
        return true;
    }

    public void LaunchGame()
    {
        _windowsHelpers.StartGame();

        ShowToast("Game Launched", MessageType.Info);
    }

    public async Task DevExpress()
    {
        if (await Stage() && await Package() && await Copy())
        {
            LaunchGame();
        }
    }

    public void OpenModsFolder()
    {
        //TODO: Fix this
        
        try
        {
            //_windowsHelpers.OpenFolder(Constants.PaksFolder);
        }
        catch (Exception)
        {
            //ShowToast($"Error opening asset folder [{Constants.PaksFolder}]", MessageType.Warning);
        }
    }
    
    public void ClearLegacyMods()
    {
        //TODO: Fix this
        
        try
        {
            //Directory.Delete(Constants.PaksFolder, true);
            //Directory.CreateDirectory(Constants.PaksFolder);
            
            ShowToast("Legacy Mods Cleared.", MessageType.Info);
        }
        catch (Exception)
        {
            ShowToast("Failed to clear mods folder. Check if game is running.", MessageType.Warning);
        }
    }

    public void OpenAssetsFolder(Guid projectId)
    {
        var path = _appDataOptions.Value.Projects
            .First(p => p.Id == projectId)
            .CookedAssetsPath;

        try
        {
            _windowsHelpers.OpenFolder(path);
        }
        catch (Exception)
        {
            ShowToast($"Error opening asset folder [{path}]", MessageType.Warning);
        }
    }

    public void OpenDistFolder(Guid projectId)
    {
        var path = _appDataOptions.Value.Projects
            .First(p => p.Id == projectId)
            .OutputPath;

        try
        {
            _windowsHelpers.OpenFolder(path);
        }
        catch (Exception)
        {
            ShowToast($"Error opening dist folder [{path}]", MessageType.Warning);
        }
    }
    
    public void AddUid()
    {
        _appDataOptions.Update(options =>
        {
            var newUid = new UidData
            {
                Name = $"Uid #{options.Uids.Count}"
            };

            options.Uids.Add(newUid);
        });
    }

    public void RemoveUid(Guid? uidId)
    {
        if (uidId == null)
        {
            ShowToast("No UID selected.", MessageType.Warning);
        }
        
        _appDataOptions.Update(options =>
        {
            options.Uids = options.Uids
                .Where(g => g.Id != uidId)
                .ToList();
        });
    }

    public IReadOnlyCollection<UidData> GetUids()
    {
        return _appDataOptions.Value.Uids;
    }

    public void AddGuid()
    {
        _appDataOptions.Update(options =>
        {
            var newGuid = new GuidData
            {
                Name = $"Guid #{options.Guids.Count}"
            };

            options.Guids.Add(newGuid);
        });
    }

    public void RemoveGuid(Guid? guidId)
    {
        if (guidId == null)
        {
            ShowToast("No GUID selected.", MessageType.Warning);
        }
        
        _appDataOptions.Update(options =>
        {
            options.Guids = options.Guids
                .Where(g => g.Id != guidId)
                .ToList();
        });
    }

    public IReadOnlyCollection<GuidData> GetGuids()
    {
        return _appDataOptions.Value.Guids;
    }

    public void UpdateGuid(GuidData? guidData)
    {
        if (guidData == null)
        {
            return;
        }
        
        _appDataOptions.Update(options =>
        {
            var guid = options.Guids
                .First(g => g.Id == guidData.Id);

            guid.Name = guidData.Name;
            guid.RetailGuid = guidData.RetailGuid;
            guid.ModdedGuid = guidData.ModdedGuid;
        });
    }

    public void UpdateUid(UidData? uidData)
    {
        if (uidData == null)
        {
            return;
        }
        
        _appDataOptions.Update(options =>
        {
            var uid = options.Uids
                .First(g => g.Id == uidData.Id);

            uid.Name = uidData.Name;
            uid.RetailUid = uidData.RetailUid;
            uid.ModdedUid = uidData.ModdedUid;
        });
    }

    private void ShowToast(string message, MessageType type)
    {
        switch (type)
        {
            case MessageType.Error:
                _snackbar.Add(message, Severity.Error, key: Guid.NewGuid().ToString("N"));

                break;
            case MessageType.Warning:
                _snackbar.Add(message, Severity.Warning, key: Guid.NewGuid().ToString("N"));
                
                break;
            case MessageType.Info:
                _snackbar.Add(message, Severity.Info, key: Guid.NewGuid().ToString("N"));
                
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void OnAppStateUpdated()
    {
        _context.StateChanged();
    }
    
    private async Task<bool?> SaveSettingsInner()
    {
        var result = (await (await _dialogService.ShowAsync<SaveConfigDialog>()!).Result)!.Data.As<SaveConfigResult?>();

        if (result == null)
        {
            return false;
        }

        if (result.Save == false)
        {
            return null;
        }

        var contents = await _appConfiguration.ExportConfig(result.PartialType);

        if (contents == null)
        {
            return false;
        }

        //TODO: Fix this
        var exporterHelperResult = false;//await _blazorOptions.Value.ConfigExportHelper(result.PartialType, contents);

        return exporterHelperResult;
    }*/
}