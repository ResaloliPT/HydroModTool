using System.Reflection;
using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.Configuration.Legacy;
using HydroToolChain.App.Configuration.Models;
using Newtonsoft.Json;

namespace HydroToolChain.App.Configuration;

internal class AppConfiguration : IAppConfiguration
{
    public static readonly string FileRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    public static readonly string AppDataFilePath = Path.Combine(FileRoot, "appData.json");
    
    private readonly IWritableOptions<AppData> _appData;

    public AppConfiguration(
        IWritableOptions<AppData> appData
        )
    {
        _appData = appData;
    }

    public async Task<(bool success, string content)> ExportConfig(ConfigPartials? partial, CancellationToken ct)
    {
        var fileContents = partial switch
        {
            ConfigPartials.Guids => JsonConvert.SerializeObject(new ConfigPartial
            {
                PartialType = ConfigPartials.Guids, Guids = _appData.Value.Guids
            }, Formatting.Indented),
            ConfigPartials.Uids => JsonConvert.SerializeObject(new ConfigPartial
            {
                PartialType = ConfigPartials.Uids, Uids = _appData.Value.Uids
            }, Formatting.Indented),
            null => JsonConvert.SerializeObject(_appData.Value, Formatting.Indented),
            _ => null
        };

        if (fileContents == null || fileContents.Trim().Equals(string.Empty))
        {
            return (false, string.Empty);
        }

        return await Task.FromResult((true, fileContents));
    }

    public async Task<bool> TryImport(string filePath)
    {
        var fileContents = "";
        
        try
        {
            fileContents = await File.ReadAllTextAsync(filePath);
        }
        catch (Exception)
        {
           // Ignored
        }

        if (fileContents.Trim().Equals(string.Empty))
        {
            return false;
        }

        #region Legacy

        if (await TryLoadLegacyGuids(fileContents))
        {
            return true;
        }
        
        if (await TryLoadLegacyUids(fileContents))
        {
            return true;
        }
        
        if (await TryLoadLegacyProjects(fileContents))
        {
            return true;
        }

        #endregion

        #region Current configs

        if (await TryLoadPartial(fileContents))
        {
            return true;
        }
        
        if (await TryLoadBackup(fileContents))
        {
            return true;
        }
        
        #endregion

        return false;
    }

    public async Task ReloadAppData(AppData data, CancellationToken ct)
    {
        var config = JsonConvert.SerializeObject(data, Formatting.Indented);

        await using var file = File.Open(AppDataFilePath, FileMode.Truncate);
        await using var writer = new StreamWriter(file);
        await writer.WriteAsync(config);
        await writer.FlushAsync();
        writer.Close();
        
        _appData.RequestUpdate();
    }

    private async Task<bool> TryLoadBackup(string fileContents)
    {
        try
        {
            var backup = JsonConvert.DeserializeObject<AppData>(fileContents, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });

            if (backup == null)
            {
                return false;
            }

            await _appData.Update(options =>
            {
                options.Guids = UpdateGuids(options.Guids, backup.Guids);
                options.Uids = UpdateUids(options.Uids, backup.Uids);
                
                var updatedProjects = options.Projects;
                var newProjects = backup.Projects
                    .Where(backupProject => updatedProjects.All(project => project.Id != backupProject.Id))
                    .Select(backupProject => new ProjectData(backupProject.Id)
                    {
                        Name = backupProject.Name,
                        ModIndex = backupProject.ModIndex,
                        CookedAssetsPath = backupProject.CookedAssetsPath,
                        OutputPath = backupProject.OutputPath,
                        Items = UpdateProjectItems(new List<ProjectItemData>(), backupProject.Items)
                        
                    })
                    .ToList();
                
                updatedProjects.AddRange(newProjects);
                updatedProjects = updatedProjects
                    .Where(project => backup.Projects.Any(backupProject => backupProject.Id == project.Id))
                    .Select(project =>
                    {
                        var backupProject = backup.Projects
                            .First(backupProject => backupProject.Id == project.Id);

                        project.Name = backupProject.Name;
                        project.ModIndex = backupProject.ModIndex;
                        project.CookedAssetsPath = backupProject.CookedAssetsPath;
                        project.OutputPath = backupProject.OutputPath;
                        project.Items = UpdateProjectItems(project.Items, backupProject.Items);
                        
                        return project;
                    })
                    .ToList();
                
                options.Projects = updatedProjects
                    .DistinctBy(project => project.Id)
                    .ToList();
            });
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private async Task<bool> TryLoadPartial(string fileContents)
    {
        try
        {
            var partial = JsonConvert.DeserializeObject<ConfigPartial>(fileContents, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });

            if (partial == null)
            {
                return false;
            }
            
            await _appData.Update(options =>
            {
                if (partial.PartialType == ConfigPartials.Guids)
                {
                    options.Guids = UpdateGuids(options.Guids, partial.Guids!);
                    
                    return;
                }

                options.Uids = UpdateUids(options.Uids, partial.Uids!);

            });
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private List<UidData> UpdateUids(List<UidData> optionsUids, List<UidData> partialUids)
    {
        var updatedUids = optionsUids;
        var newUids = partialUids
            .Where(partialUid => updatedUids.All(uid => uid.Id != partialUid.Id))
            .Select(partialUid => new UidData(partialUid.Id)
            {
                Name = partialUid.Name,
                ModdedUid = partialUid.ModdedUid,
                RetailUid = partialUid.RetailUid
            })
            .ToList();
                
        updatedUids.AddRange(newUids);
        updatedUids = updatedUids
            .Where(uid => partialUids.Any(partialUid => partialUid.Id == uid.Id))
            .Select(uid =>
            {
                var partialUid = partialUids
                    .First(partialUid => partialUid.Id == uid.Id);

                uid.Name = partialUid.Name;
                uid.ModdedUid = partialUid.ModdedUid;
                uid.RetailUid = partialUid.RetailUid;
                return uid;
            })
            .ToList();
        
        return updatedUids
            .DistinctBy(uid => uid.Id)
            .ToList();
    }

    private List<GuidData> UpdateGuids(List<GuidData> current, List<GuidData> guids)
    {
        var updatedGuids = current;
        var newGuids = guids
            .Where(partialGuid => updatedGuids.All(guid => guid.Id != partialGuid.Id))
            .Select(partialGuid => new GuidData(partialGuid.Id)
            {
                Name = partialGuid.Name,
                ModdedGuid = partialGuid.ModdedGuid,
                RetailGuid = partialGuid.RetailGuid
            })
            .ToList();
                
        updatedGuids.AddRange(newGuids);
        updatedGuids = updatedGuids
            .Where(guid => guids.Any(partialGuid => partialGuid.Id == guid.Id))
            .Select(guid =>
            {
                var partialGuid = guids
                    .First(partialGuid => partialGuid.Id == guid.Id);

                guid.Name = partialGuid.Name;
                guid.ModdedGuid = partialGuid.ModdedGuid;
                guid.RetailGuid = partialGuid.RetailGuid;
                return guid;
            })
            .ToList();
        
        return updatedGuids
            .DistinctBy(guid => guid.Id)
            .ToList();
    }

    private async Task<bool> TryLoadLegacyProjects(string fileContents)
    {
        try
        {
            //Import Legacy Projects
            
            var legacyProjects = JsonConvert.DeserializeObject<LegacyProjects>(fileContents, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });

            if (legacyProjects == null)
            {
                return false;
            }

            await _appData.Update(options =>
            {
                var updatedProjects = options.Projects;
                var newProjects = legacyProjects.AppConfiguration.Projects
                    .Where(legacyProject => updatedProjects.All(project => project.Id != legacyProject.Id))
                    .Select(legacyProject => new ProjectData(legacyProject.Id)
                    {
                        Name = legacyProject.Name,
                        ModIndex = legacyProject.ModIndex,
                        CookedAssetsPath = legacyProject.Path,
                        OutputPath = legacyProject.OutputPath,
                        Items = UpdateProjectItems(new List<ProjectItemData>(), legacyProject.Items)
                        
                    })
                    .ToList();
                
                updatedProjects.AddRange(newProjects);
                
                options.Projects = updatedProjects
                    .DistinctBy(project => project.Id)
                    .ToList();

                options.CurrentProject = options.CurrentProject == Guid.Empty || options.Projects
                    .All(p => p.Id != options.CurrentProject)
                    ? options.Projects.FirstOrDefault()?.Id ?? Guid.Empty
                    : Guid.Empty;
            });
            
            return true;
        }
        catch (Exception)
        {
            // Ignored
        }

        return false;
    }

    private List<ProjectItemData> UpdateProjectItems(List<ProjectItemData> toUpdate, List<ProjectItemData> source)
    {
        var updatedProjects = toUpdate;
        var newProjects = source
            .Where(backupProjectItems => updatedProjects.All(project => project.Id != backupProjectItems.Id))
            .Select(backupProjectItems => new ProjectItemData(backupProjectItems.Id)
            {
                Name = backupProjectItems.Name,
                Path = backupProjectItems.Path
            })
            .ToList();
                
        updatedProjects.AddRange(newProjects);
        updatedProjects = updatedProjects
            .Where(project => source.Any(backupProjectItems => backupProjectItems.Id == project.Id))
            .Select(project =>
            {
                var legacyProjectItems = source
                    .First(backupProjectItems => backupProjectItems.Id == project.Id);

                project.Name = legacyProjectItems.Name;
                project.Path = legacyProjectItems.Path;
                return project;
            })
            .ToList();
                
        return updatedProjects
            .DistinctBy(project => project.Id)
            .ToList();
    }
    
    private List<ProjectItemData> UpdateProjectItems(List<ProjectItemData> toUpdate, List<Items> source)
    {
        var updatedProjects = toUpdate;
        var newProjects = source
            .Where(legacyProjectItems => updatedProjects.All(project => project.Id != legacyProjectItems.Id))
            .Select(legacyProjectItems => new ProjectItemData(legacyProjectItems.Id)
            {
                Name = legacyProjectItems.Name,
                Path = legacyProjectItems.Path
            })
            .ToList();
                
        updatedProjects.AddRange(newProjects);
        updatedProjects = updatedProjects
            .Where(project => source.Any(legacyProjectItems => legacyProjectItems.Id == project.Id))
            .Select(project =>
            {
                var legacyProjectItems = source
                    .First(legacyProjectItems => legacyProjectItems.Id == project.Id);

                project.Name = legacyProjectItems.Name;
                project.Path = legacyProjectItems.Path;
                return project;
            })
            .ToList();
                
        return updatedProjects
            .DistinctBy(project => project.Id)
            .ToList();
    }

    private async Task<bool> TryLoadLegacyUids(string fileContents)
    {
        try
        {
            //Import Legacy Uids
            
            var legacyUids = JsonConvert.DeserializeObject<LegacyUids>(fileContents, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });

            if (legacyUids == null)
            {
                return false;
            }

            await _appData.Update(options =>
            {
                var updatedUids = options.Uids;
                var newUids = legacyUids.UIDConfiguration.UIDs
                    .Where(legacyUid => updatedUids.All(uid => uid.Id != legacyUid.Id))
                    .Select(legacyUid => new UidData(legacyUid.Id)
                    {
                        Name = legacyUid.Name,
                        ModdedUid = legacyUid.ModdedUID,
                        RetailUid = legacyUid.OriginalUID
                    })
                    .ToList();
                
                updatedUids.AddRange(newUids);
                updatedUids = updatedUids
                    .Where(uid => legacyUids.UIDConfiguration.UIDs.Any(legacyUid => legacyUid.Id == uid.Id))
                    .Select(uid =>
                    {
                        var legacyUid = legacyUids.UIDConfiguration.UIDs
                            .First(legacyUid => legacyUid.Id == uid.Id);

                        uid.Name = legacyUid.Name;
                        uid.ModdedUid = legacyUid.ModdedUID;
                        uid.RetailUid = legacyUid.OriginalUID;
                        return uid;
                    })
                    .ToList();
                
                options.Uids = updatedUids
                    .DistinctBy(uid => uid.Id)
                    .ToList();
            });
            
            return true;
        }
        catch (Exception)
        {
            // Ignored
        }

        return false;
    }

    private async Task<bool> TryLoadLegacyGuids(string fileContents)
    {
        try
        {
            //Import Legacy Guids
            
            var legacyGuids = JsonConvert.DeserializeObject<LegacyGuids>(fileContents, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });

            if (legacyGuids == null)
            {
                return false;
            }

            await _appData.Update(options =>
            {
                var updatedGuids = options.Guids;
                var newGuids = legacyGuids.GuidConfiguration.Guids
                    .Where(legacyGuid => updatedGuids.All(guid => guid.Id != legacyGuid.Id))
                    .Select(legacyGuid => new GuidData(legacyGuid.Id)
                    {
                        Name = legacyGuid.Name,
                        ModdedGuid = legacyGuid.ModdedGuid,
                        RetailGuid = legacyGuid.OriginalGuid
                    })
                    .ToList();
                
                updatedGuids.AddRange(newGuids);
                updatedGuids = updatedGuids
                    .Where(guid => legacyGuids.GuidConfiguration.Guids.Any(legacyGuid => legacyGuid.Id == guid.Id))
                    .Select(guid =>
                    {
                        var legacyGuid = legacyGuids.GuidConfiguration.Guids
                            .First(legacyGuid => legacyGuid.Id == guid.Id);

                        guid.Name = legacyGuid.Name;
                        guid.ModdedGuid = legacyGuid.ModdedGuid;
                        guid.RetailGuid = legacyGuid.OriginalGuid;
                        return guid;
                    })
                    .ToList();
                
                options.Guids = updatedGuids
                    .DistinctBy(guid => guid.Id)
                    .ToList();
            });
            
            return true;
        }
        catch (Exception)
        {
            // Ignored
        }

        return false;
    }
}