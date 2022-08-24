using NuGet.Versioning;

namespace DotNetOutdated.Core.Services;

public interface ICentralPackageVersionManagementService
{
    Task<RunStatus> AddPackageAsync(string projectFilePath, string packageName, NuGetVersion version);
}
