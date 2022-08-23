using DotNetOutdated.Core;
using DotNetOutdated.Core.Models;
using DotNetOutdated.Core.Services;
using Moq;
using NuGet.Frameworks;
using NuGet.Versioning;
using Xunit;

namespace DotNetOutdated.Tests;

public class NuGetPackageResolutionServiceTests
{
    private const string PackageName = "MyPackage";
    private readonly NuGetPackageResolutionService _nuGetPackageResolutionService;

    public NuGetPackageResolutionServiceTests()
    {
        var availableVersions = new List<NuGetVersion>
        {
            new("1.1.0"),
            new("1.2.0"),
            new("1.2.2"),
            new("1.3.0-pre"),
            new("1.3.0"),
            new("1.4.0-pre"),
            new("2.0.0"),
            new("2.1.0"),
            new("2.2.0-pre.1"),
            new("2.2.0-pre.2"),
            new("3.0.0-pre.1"),
            new("3.0.0-pre.2"),
            new("3.1.0-pre.1"),
            new("4.0.0-pre.1")
        };

        var nuGetPackageInfoService = new Mock<INuGetPackageInfoService>();
        nuGetPackageInfoService.Setup(service => service.GetAllVersions(PackageName, It.IsAny<List<Uri>>(), It.IsAny<bool>(), It.IsAny<NuGetFramework>(), It.IsAny<string>(), It.IsAny<bool>(), 0, It.IsAny<bool>()))
            .ReturnsAsync(availableVersions);

        _nuGetPackageResolutionService = new NuGetPackageResolutionService(nuGetPackageInfoService.Object);
    }

    [Theory]
    [InlineData("1.2.0", VersionLock.None, PrereleaseReporting.Auto, "2.1.0")]
    [InlineData("1.2.0", VersionLock.None, PrereleaseReporting.Always, "4.0.0-pre.1")]
    [InlineData("1.3.0-pre", VersionLock.None, PrereleaseReporting.Auto, "4.0.0-pre.1")]
    [InlineData("1.3.0-pre", VersionLock.None, PrereleaseReporting.Never, "2.1.0")]
    [InlineData("1.2.0", VersionLock.Major, PrereleaseReporting.Auto, "1.3.0")]
    [InlineData("1.2.0", VersionLock.Minor, PrereleaseReporting.Auto, "1.2.2")]
    [InlineData("3.0.0-pre.1", VersionLock.None, PrereleaseReporting.Never, "3.0.0-pre.1")]
    [InlineData("3.0.0-pre.1", VersionLock.None, PrereleaseReporting.Always, "4.0.0-pre.1")]
    [InlineData("3.0.0-pre.1", VersionLock.None, PrereleaseReporting.Auto, "4.0.0-pre.1")]
    [InlineData("3.0.0-pre.1", VersionLock.Minor, PrereleaseReporting.Auto, "3.0.0-pre.2")]
    [InlineData("3.0.0-pre.1", VersionLock.Minor, PrereleaseReporting.Always, "3.0.0-pre.2")]
    [InlineData("3.0.0-pre.1", VersionLock.Major, PrereleaseReporting.Auto, "3.1.0-pre.1")]
    [InlineData("3.0.0-pre.1", VersionLock.Major, PrereleaseReporting.Always, "3.1.0-pre.1")]
    public async Task ResolvesVersion_Correctly(string current, VersionLock versionLock, PrereleaseReporting prerelease, string latest)
    {
        // Arrange
        var model = new CommandModel
        {
            VersionLock = versionLock,
            PreRelease = prerelease,
        };

        // Act
        var latestVersion = await _nuGetPackageResolutionService.ResolvePackageVersions(PackageName, NuGetVersion.Parse(current), new List<Uri>(), VersionRange.Parse(current), model, null!, null!, false).ConfigureAwait(false);

        // Assert
        Assert.Equal(NuGetVersion.Parse(latest), latestVersion);
    }
}
