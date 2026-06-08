using Microsoft.Build.Utilities;

using NuGet.Common;

namespace FredrikHr.NupkgMSBuild.PackageAuthorSigning;

internal sealed class MSBuildNuGetLoggingAdapter(TaskLoggingHelper logger)
    : LoggerBase()
{
    public override void Log(ILogMessage message)
    {
        switch (message.Level)
        {
            case LogLevel.Error:
                logger.LogError(
                    message: message.Message,
                    errorCode: message.Code.ToString(),
                    file: message.ProjectPath,
                    subcategory: default,
                    helpKeyword: default,
                    lineNumber: default,
                    columnNumber: default,
                    endLineNumber: default,
                    endColumnNumber: default
                );
                break;
            case LogLevel.Warning:
                logger.LogWarning(
                    message: message.Message,
                    warningCode: message.Code.ToString(),
                    file: message.ProjectPath,
                    subcategory: default,
                    helpKeyword: default,
                    lineNumber: default,
                    columnNumber: default,
                    endLineNumber: default,
                    endColumnNumber: default
                );
                break;
            case LogLevel.Minimal:
                logger.LogMessage(
                    message: message.Message,
                    code: message.Code.ToString(),
                    file: message.ProjectPath,
                    subcategory: default,
                    helpKeyword: default,
                    lineNumber: default,
                    columnNumber: default,
                    endLineNumber: default,
                    endColumnNumber: default,
                    importance: Microsoft.Build.Framework.MessageImportance.High
                );
                break;
            case LogLevel.Information:
                logger.LogMessage(
                    message: message.Message,
                    code: message.Code.ToString(),
                    file: message.ProjectPath,
                    subcategory: default,
                    helpKeyword: default,
                    lineNumber: default,
                    columnNumber: default,
                    endLineNumber: default,
                    endColumnNumber: default,
                    importance: Microsoft.Build.Framework.MessageImportance.Normal
                );
                break;
            case LogLevel.Verbose:
                logger.LogMessage(
                    message: message.Message,
                    code: message.Code.ToString(),
                    file: message.ProjectPath,
                    subcategory: default,
                    helpKeyword: default,
                    lineNumber: default,
                    columnNumber: default,
                    endLineNumber: default,
                    endColumnNumber: default,
                    importance: Microsoft.Build.Framework.MessageImportance.Low
                );
                break;
        }
    }

    public override System.Threading.Tasks.Task LogAsync(ILogMessage message)
    {
        Log(message);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}