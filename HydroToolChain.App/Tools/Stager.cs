using HydroToolChain.App.Configuration.Data;
using HydroToolChain.App.Models;
using Microsoft.Extensions.Options;

namespace HydroToolChain.App.Tools;

public class Stager
{
    private readonly IOptions<AppOptions> _options;

    public Stager(
        IOptions<AppOptions> options
    )
    {
        _options = options;
    }
    
    internal Task StageAsync(ProjectData project, IReadOnlyCollection<GuidData> guids, IReadOnlyCollection<UidData> uids)
    {
        return Task.Run(() =>
        {
            var basePathSrc = project.CookedAssetsPath;
            var basePathOut = Path.Combine(project.OutputPath, "Staging", project.Name, "Mining");
            
            if (Directory.Exists(basePathOut))
            {
                Directory.Delete(basePathOut, true);
            }

            var missingFiles = VerifyFiles(basePathSrc, project.Items);

            if (missingFiles.Count > 0)
            {
                _options.Value.ShowMessage(
                    MessageType.Error, @$"These assets are missing: \n{string.Join("\n", missingFiles)} \n Staging aborted");

                return;
            }

            foreach (var projectItem in project.Items)
            {
                var newPath = projectItem.Path.Replace(basePathSrc, basePathOut);
                Directory.CreateDirectory(Path.GetDirectoryName(basePathOut + newPath)!);

                // ReSharper disable once StringLiteralTypo
                if (projectItem.Name.EndsWith(".uexp"))
                {
                    var patched = PatchFile(basePathSrc + projectItem.Path, guids, Array.Empty<UidData>());

                    if (patched == null)
                    {
                        File.Copy(basePathSrc + projectItem.Path, basePathOut + newPath, true);

                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(basePathOut + newPath)!);

                    using var file = File.Create(basePathOut + newPath, (int)patched.Length, FileOptions.Asynchronous | FileOptions.SequentialScan);
                    patched.Position = 0;
                    patched.CopyTo(file);
                }
                // ReSharper disable once StringLiteralTypo
                else if (projectItem.Name.EndsWith(".uasset"))
                {
                    var patched = PatchFile(basePathSrc + projectItem.Path, Array.Empty<GuidData>(), uids);

                    if (patched == null)
                    {
                        File.Copy(basePathSrc + projectItem.Path, basePathOut + newPath, true);

                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(basePathOut + newPath)!);

                    using var file = File.Create(basePathOut + newPath, (int)patched.Length, FileOptions.Asynchronous | FileOptions.SequentialScan);
                    patched.Position = 0;
                    patched.CopyTo(file);
                }
                else
                {
                    File.Copy(basePathSrc + projectItem.Path, basePathOut + newPath, true);
                }
            }
        });
    }

    private static IReadOnlyCollection<string> VerifyFiles(string basePath, IReadOnlyCollection<ProjectItemData> items)
    {
        var itemList = new List<ProjectItemData>(items);

        return itemList.Where(item => {
            var itemPath = basePath + item.Path;

            return !File.Exists(itemPath);
        }).Select(item => basePath + item.Path).ToList();
    }

    private static MemoryStream? PatchFile(string fileSrc, IReadOnlyCollection<GuidData> guids, IReadOnlyCollection<UidData> uids)
    {
        var fileBytes = File.ReadAllBytes(fileSrc);

        var patchedBytes = new byte[fileBytes.Length];
        Buffer.BlockCopy(fileBytes, 0, patchedBytes, 0, patchedBytes.Length);

        var wasPatched = false;
        foreach (var guid in guids)
        {
            var moddedGuidBytes = Utilities.Hex2Binary(guid.ModdedGuid.ToString("N"));
            var originalGuidBytes = Utilities.Hex2Binary(guid.RetailGuid.ToString("N"));

            var positions = Utilities.SearchBytePattern(moddedGuidBytes, patchedBytes);

            if (positions.Count == 0)
            {
                continue; 
            }

            var position = positions.First();

            var tmpPatchedBytes = new byte[patchedBytes.Length];

            Buffer.BlockCopy(patchedBytes, 0, tmpPatchedBytes, 0, position);
            Buffer.BlockCopy(originalGuidBytes, 0, tmpPatchedBytes, position, originalGuidBytes.Length);
            Buffer.BlockCopy(patchedBytes, position + originalGuidBytes.Length, tmpPatchedBytes, position + originalGuidBytes.Length, patchedBytes.Length - (position + originalGuidBytes.Length));
            
            Buffer.BlockCopy(tmpPatchedBytes, 0, patchedBytes, 0, patchedBytes.Length);
            
            wasPatched = true;
        }

        foreach (var uid in uids)
        {
            var moddedUid = Utilities.String2Binary(uid.ModdedUid);
            var retailUid = Utilities.String2Binary(uid.RetailUid);

            var namePositions = Utilities.SearchBytePattern(moddedUid, patchedBytes);

            if (namePositions.Count > 0)
            {
                foreach (var namePosition in namePositions)
                {
                    var tmpPatchedBytes = new byte[patchedBytes.Length];

                    Buffer.BlockCopy(patchedBytes, 0, tmpPatchedBytes, 0, namePosition);
                    Buffer.BlockCopy(retailUid, 0, tmpPatchedBytes, namePosition, retailUid.Length);
                    Buffer.BlockCopy(patchedBytes, namePosition + retailUid.Length, tmpPatchedBytes, namePosition + retailUid.Length, patchedBytes.Length - (namePosition + retailUid.Length));

                    Buffer.BlockCopy(tmpPatchedBytes, 0, patchedBytes, 0, patchedBytes.Length);
                    
                    wasPatched = true;
                }
            }
        }

        if (!wasPatched)
        {
            return null;
        }

        var ms = new MemoryStream(patchedBytes);

        return ms;
    }
}
