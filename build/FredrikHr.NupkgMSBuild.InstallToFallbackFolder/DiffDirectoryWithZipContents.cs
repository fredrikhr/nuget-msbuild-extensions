#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace FredrikHr.NupkgMSBuild.InstallToFallbackFolder;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1819: Properties should not return arrays",
    Justification = nameof(MSBuildTask)
    )]
[MSBuildMultiThreadableTask]
public sealed class DiffDirectoryWithZipContents : IMultiThreadableTask
{
    public TaskEnvironment TaskEnvironment { get; set; } = default!;
    public IBuildEngine BuildEngine { get; set; } = default!;
    public ITaskHost HostObject { get; set; } = default!;

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

    public bool Execute()
    {
        AbsolutePath rootPath = DestinationFolder?.GetMetadata("FullPath") switch
        {
            string fullPath when !string.IsNullOrEmpty(fullPath) =>
                TaskEnvironment.GetAbsolutePath(fullPath),
            _ => TaskEnvironment.ProjectDirectory,
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
                AbsolutePath zipPath = new(zipEntry.FullName, rootPath);
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
        return TaskEnvironment.GetAbsolutePath(item.GetMetadata("FullPath"));
    }
}