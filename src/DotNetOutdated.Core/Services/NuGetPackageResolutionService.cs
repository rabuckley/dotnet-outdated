using System.Collections.Concurrent;
using DotNetOutdated.Core.Models;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace DotNetOutdated.Core.Services;

public class NuGetPackageResolutionService : INuGetPackageResolutionService
{
    private readonly INuGetPackageInfoService _nugetService;
    private readonly ConcurrentDictionary<string, Lazy<Task<IReadOnlyList<NuGetVersion>>>> _cache = new();

    public NuGetPackageResolutionService(INuGetPackageInfoService nugetService)
    {
        _nugetService = nugetService;
    }

    public async Task<NuGetVersion> ResolvePackageVersions(string packageName, NuGetVersion referencedVersion, IEnumerable<Uri> sources, VersionRange currentVersionRange, CommandModel model, NuGetFramework targetFrameworkName, string projectFilePath, bool isDevelopmentDependency)
    {
        return await ResolvePackageVersions(packageName, referencedVersion, sources, currentVersionRange, targetFrameworkName, projectFilePath, isDevelopmentDependency, model).ConfigureAwait(false);
    }
    public async Task<NuGetVersion> ResolvePackageVersions(string packageName, NuGetVersion referencedVersion, IEnumerable<Uri> sources, VersionRange currentVersionRange, NuGetFramework targetFrameworkName, string projectFilePath, bool isDevelopmentDependency, CommandModel model)
    {
        var includePrerelease = model.PreRelease switch
        {
            PrereleaseReporting.Always => true,
            PrereleaseReporting.Never => false,
            _ => referencedVersion.IsPrerelease
        };

        var cacheKey = (packageName + "-" + includePrerelease + "-" + targetFrameworkName + "-" + model.OlderThanDays).ToLowerInvariant();

        var allVersionsRequest = new Lazy<Task<IReadOnlyList<NuGetVersion>>>(() => _nugetService.GetAllVersions(packageName, sources, includePrerelease, targetFrameworkName, projectFilePath, isDevelopmentDependency, model.OlderThanDays, model.IgnoreFailedSources));
        var allVersions = await _cache.GetOrAdd(cacheKey, allVersionsRequest).Value.ConfigureAwait(false);

        var floatingBehaviour = model.VersionLock switch
        {
            VersionLock.Major => includePrerelease ? NuGetVersionFloatBehavior.PrereleaseMinor : NuGetVersionFloatBehavior.Minor,
            VersionLock.Minor => includePrerelease ? NuGetVersionFloatBehavior.PrereleasePatch : NuGetVersionFloatBehavior.Patch,
            _ => includePrerelease ? NuGetVersionFloatBehavior.AbsoluteLatest : NuGetVersionFloatBehavior.Major
        };

        var releasePrefix = string.Empty;
        if (referencedVersion.IsPrerelease)
        {
            releasePrefix = referencedVersion.ReleaseLabels.First(); // TODO Not sure exactly what to do for this bit
        }

        // Create a new version range for comparison
        var latestVersionRange = new VersionRange(currentVersionRange, new FloatRange(floatingBehaviour, referencedVersion, releasePrefix));

        // Use new version range to determine latest version
        var latestVersion = latestVersionRange.FindBestMatch(allVersions);

        return latestVersion;
    }
}
