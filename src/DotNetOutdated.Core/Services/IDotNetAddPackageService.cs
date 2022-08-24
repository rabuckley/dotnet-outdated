using NuGet.Versioning;

namespace DotNetOutdated.Core.Services;

public interface IDotNetAddPackageService
{
    Task<RunStatus> AddPackage(string projectPath, string packageName, string frameworkName, NuGetVersion version, bool ignoreFailedSources);
}
