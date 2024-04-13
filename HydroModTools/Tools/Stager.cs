using HydroModTools.Common.Models;
using HydroModTools.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HydroModTools.Tools
{
    public class Stager
    {
        public static Task StageAsync(Action<ProgressbarStateModel> reportProgress, int progressMin, int progressMax, ProjectModel project, IReadOnlyCollection<GuidItemModel> guids, IReadOnlyCollection<UIDItemModel> dtUIDs)
        {
            var basePathSrc = project.Path;
            var basePathOut = Path.Combine(project.OutputPath, "Staging", project.Name, "Mining");

            reportProgress.Invoke(new ProgressbarStateModel((int)Math.Floor(Utilities.Remap(10, 0, 100, progressMin, progressMax)), "Checking path"));

            if (Directory.Exists(basePathOut))
            {
                Directory.Delete(basePathOut, true);
            }

            Directory.CreateDirectory(basePathOut);

            var missingFiles = VerifyFiles(basePathSrc, project.Items);

            if (missingFiles.Count > 0)
            {
                MessageBox.Show(@$"These assets are missing: \n{string.Join("\n", missingFiles)} \n Stating aborted", @"Missing Assets");

                return Task.CompletedTask;
            }

            var count = 0;
            foreach (var projectItem in project.Items)
            {
                reportProgress.Invoke(new ProgressbarStateModel((int)Math.Floor(Utilities.Remap((decimal)count, 0, (decimal)project.Items.Count, progressMin, progressMax)), $"Staging {count + 1}/{project.Items.Count} ({projectItem.Name})"));

                var newPath = projectItem.Path.Replace(basePathSrc, basePathOut);
                Directory.CreateDirectory(Path.GetDirectoryName(basePathOut + newPath)!);

                // ReSharper disable once StringLiteralTypo
                if (projectItem.Name.EndsWith(".uexp"))
                {
                    var patched = PatchFile(basePathSrc + projectItem.Path, guids, Array.Empty<UIDItemModel>());

                    if (patched == null)
                    {
                        File.Copy(basePathSrc + projectItem.Path, basePathOut + newPath, true);
                        count++;

                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(basePathOut + newPath)!);

                    using var file = File.Create(basePathOut + newPath, (int)patched.Length, FileOptions.Asynchronous | FileOptions.SequentialScan);
                    patched.Position = 0;
                    patched.CopyTo(file);
                }
                else if (projectItem.Name.EndsWith(".uasset"))
                {
                    var patched = PatchFile(basePathSrc + projectItem.Path, Array.Empty<GuidItemModel>(), dtUIDs);

                    if (patched == null)
                    {
                        File.Copy(basePathSrc + projectItem.Path, basePathOut + newPath, true);
                        count++;

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

                count++;
            }

            return Task.CompletedTask;
        }

        private static IReadOnlyCollection<string> VerifyFiles(string basePath, IEnumerable<ProjectItemModel> items)
        {
            var itemList = new List<ProjectItemModel>(items);

            return itemList.Where(item => {
                var itemPath = basePath + item.Path;

                return !File.Exists(itemPath);
            }).Select(item => basePath + item.Path).ToList();
        }

        private static MemoryStream? PatchFile(string fileSrc, IEnumerable<GuidItemModel> guids, IEnumerable<UIDItemModel> dtUIDs)
        {
            var fileBytes = File.ReadAllBytes(fileSrc);

            var patchedBytes = new byte[fileBytes.Length];
            Buffer.BlockCopy(fileBytes, 0, patchedBytes, 0, patchedBytes.Length);

            var wasPatched = false;
            foreach (var guid in guids)
            {
                var moddedGuidBytes = Utilities.Hex2Binary(guid.ModdedGuid.ToString("N"));
                var originalGuidBytes = Utilities.Hex2Binary(guid.OriginalGuid.ToString("N"));

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

            foreach (var dtUID in dtUIDs)
            {
                var moddedUID = Utilities.String2Binary(dtUID.ModdedUID);
                var retailUID = Utilities.String2Binary(dtUID.OriginalUID);

                var namePositions = Utilities.SearchBytePattern(moddedUID, patchedBytes);

                if (namePositions.Count > 0)
                {
                    foreach (var namePosition in namePositions)
                    {
                        var tmpPatchedBytes = new byte[patchedBytes.Length];

                        Buffer.BlockCopy(patchedBytes, 0, tmpPatchedBytes, 0, namePosition);
                        Buffer.BlockCopy(retailUID, 0, tmpPatchedBytes, namePosition, retailUID.Length);
                        Buffer.BlockCopy(patchedBytes, namePosition + retailUID.Length, tmpPatchedBytes, namePosition + retailUID.Length, patchedBytes.Length - (namePosition + retailUID.Length));

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
}
