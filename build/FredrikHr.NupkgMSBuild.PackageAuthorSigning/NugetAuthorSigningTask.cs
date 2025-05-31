using System.Security.Cryptography.X509Certificates;

using Microsoft.Build.Framework;

using NuGet.Commands;

using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace FredrikHr.NupkgMSBuild.PackageAuthorSigning;

public class NugetAuthorSigningTask : MSBuildTask
{
    [Required]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1819: Properties should not return arrays",
        Justification = nameof(Microsoft.Build.Framework.ITask)
    )]
    public required ITaskItem[] PackagePaths { get; init; }

    public string? CertificateFingerprint { get; init; }

    public ITaskItem? CertificatePath { get; init; }

    public string? CertificatePassword { get; init; }

    public string? CertificateStoreLocation { get; init; }

    public string? CertificateStoreName { get; init; }

    public string? CertificateSubjectName { get; init; }

    public ITaskItem? OutputDirectory { get; init; }

    public bool Overwrite { get; init; }

    public string? SignatureHashAlgorithm { get; init; }

    public string? Timestamper { get; init; }

    public string? TimestampHashAlgorithm { get; init; }

    public override bool Execute()
    {
        SignArgs args = new()
        {
            PackagePaths = [..PackagePaths.Select(ti => ti.GetMetadata("FullPath"))],
            CertificateFingerprint = CertificateFingerprint,
            CertificatePath = CertificatePath?.GetMetadata("FullPath"),
            CertificatePassword = CertificatePassword,
            CertificateStoreLocation = string.IsNullOrWhiteSpace(CertificateStoreLocation)
                ? StoreLocation.CurrentUser
                : ParseEnum<StoreLocation>(CertificateStoreLocation!),
            CertificateStoreName = string.IsNullOrWhiteSpace(CertificateStoreName)
                ? StoreName.My
                : ParseEnum<StoreName>(CertificateStoreName!),
            CertificateSubjectName = CertificateSubjectName,
            OutputDirectory = OutputDirectory?.GetMetadata("FullPath"),
            Overwrite = Overwrite,
            SignatureHashAlgorithm = string.IsNullOrWhiteSpace(SignatureHashAlgorithm)
                ? NuGet.Common.HashAlgorithmName.SHA256
                : ParseEnum<NuGet.Common.HashAlgorithmName>(SignatureHashAlgorithm!),
            Timestamper = Timestamper,
            TimestampHashAlgorithm = string.IsNullOrWhiteSpace(TimestampHashAlgorithm)
                ? NuGet.Common.HashAlgorithmName.SHA256
                : ParseEnum<NuGet.Common.HashAlgorithmName>(TimestampHashAlgorithm!),

            Logger = new MSBuildNuGetLoggingAdapter(Log),
            NonInteractive = true,
        };
        SignCommandRunner runner = new();
        int resultCode = runner.ExecuteCommandAsync(args)
            .GetAwaiter().GetResult();
        return resultCode == 0;
    }

    internal static T ParseEnum<T>(string value)
        where T : struct, Enum
        #if NETFRAMEWORK
        => (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        #else
        => Enum.Parse<T>(value, ignoreCase: true);
        #endif
}
