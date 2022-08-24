using DotNetOutdated.Core.Models;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace DotNetOutdated.Core.Services;

public interface INuGetPackageResolutionService
{
    Task<NuGetVersion> ResolvePackageVersions(string packageName, NuGetVersion referencedVersion, IEnumerable<Uri> sources, VersionRange currentVersionRange, NuGetFramework targetFrameworkName, string projectFilePath, bool isDevelopmentDependency, CommandModel model);
}
