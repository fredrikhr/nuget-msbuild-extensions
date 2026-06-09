#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;

using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace FredrikHr.NupkgMSBuild.ProjectReferenceAsPackageReference;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1819: Properties should not return arrays",
    Justification = nameof(Microsoft.Build.Framework)
    )]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Style",
    "IDE0305: Simplify collection initialization",
    Justification = nameof(Microsoft.Build.Framework)
    )]
public class ProjectReferenceMissingFromRestoreOutputFoldersTask : MSBuildTask
{
    [Required]
    public ITaskItem[] ProjectReferences { get; set; } = null!;

    [Required]
    public ITaskItem[] RestoreOutputFolders { get; set; } = null!;

    [Output]
    public ITaskItem[] MissingProjectReferences { get; set; } = null!;

    public override bool Execute()
    {
        bool taskResult = true;
        ITaskItem[] prjRefs = ProjectReferences ?? [];
        List<ITaskItem> missingPrjRefs = new(capacity: prjRefs.Length);
        foreach (ITaskItem prjRef in prjRefs)
        {
            string pkgId = prjRef.GetMetadata("PackageId");
            if (string.IsNullOrEmpty(pkgId))
            {
                Log.LogError(
                    message: $"Project reference item does not have required metadata value: PackageId",
                    file: prjRef.ItemSpec,
                    lineNumber: default,
                    columnNumber: default,
                    endLineNumber: default,
                    endColumnNumber: default,
                    subcategory: default,
                    errorCode: default,
                    helpKeyword: default
                    );
                taskResult = false;
                continue;
            }

            if (IsMissingProjectReference(prjRef, pkgId))
            {
                missingPrjRefs.Add(prjRef);
            }
        }
        MissingProjectReferences = missingPrjRefs.ToArray();
        return taskResult;
    }

    private bool IsMissingProjectReference(ITaskItem projectReference, string pkgId)
    {
        string? prjVer = projectReference.GetMetadata("ProjectVersion");
        string projPartialPath = string.IsNullOrEmpty(prjVer)
            ? pkgId
            : Path.Combine(pkgId, prjVer);
        return RestoreOutputFolders.All(restoreFolder =>
        {
            string containerPath = restoreFolder.GetMetadata("FullPath");
            if (string.IsNullOrEmpty(containerPath)) return true;
            string pkgPath = Path.Combine(containerPath, projPartialPath);
            return !Directory.Exists(pkgPath);
        });
    }
}