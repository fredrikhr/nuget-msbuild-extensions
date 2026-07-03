#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Microsoft.Build.Framework;

using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace FredrikHr.NupkgMSBuild.InstallToFallbackFolder;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1819: Properties should not return arrays",
    Justification = nameof(MSBuildTask)
    )]
public sealed class DiffDirectoryWithZipContents : MSBuildTask
{
    public ITaskItem? DestinationFolder { get; set; }
    public ITaskItem[] DestinationFolderFiles { get; set; } = [];
    [Required]
    public ITaskItem[] ZipArchiveFile { get; set; } = [];
    public ITaskItem[] ExtraneousFiles { get; set; } = [];

    private readonly Func<ITaskItem, string> _relativePathKeySelector;

    public DiffDirectoryWithZipContents()
    {
        _relativePathKeySelector = GetRelativePath;
    }

    public override bool Execute()
    {
        string rootPath = DestinationFolder?.GetMetadata("FullPath") switch
        {
            string fullPath when !string.IsNullOrEmpty(fullPath) => fullPath,
            _ => Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode),
        };

        Dictionary<string, ITaskItem> extraneousFiles =
            (DestinationFolderFiles ?? []).ToDictionary(
                _relativePathKeySelector,
                StringComparer.OrdinalIgnoreCase
                );

        foreach (ITaskItem zipFileItem in ZipArchiveFile ?? [])
        {
            using ZipArchive zipArchive = ZipFile.OpenRead(
                zipFileItem.GetMetadata("FullPath")
                );
            foreach (ZipArchiveEntry zipEntry in zipArchive.Entries)
            {
                string zipPath = Path.Combine(rootPath, zipEntry.FullName);
                string zipOsPath = Path.GetFullPath(zipPath);
                extraneousFiles.Remove(zipOsPath);
            }

            if (extraneousFiles.Count == 0) break;
        }

        ExtraneousFiles = [.. extraneousFiles.Values];
        return true;
    }

    private string GetRelativePath(ITaskItem item)
    {
        return item.GetMetadata("FullPath");
    }
}
