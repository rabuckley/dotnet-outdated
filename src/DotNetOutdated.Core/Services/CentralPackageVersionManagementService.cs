using System.IO.Abstractions;
using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace DotNetOutdated.Core.Services;

public class CentralPackageVersionManagementService : ICentralPackageVersionManagementService
{
    private readonly IFileSystem _fileSystem;
    private readonly IDotNetRestoreService _dotNetRestoreService;

    public CentralPackageVersionManagementService(IFileSystem fileSystem, IDotNetRestoreService dotNetRestoreService)
    {
        _fileSystem = fileSystem;
        _dotNetRestoreService = dotNetRestoreService;
    }

    public RunStatus AddPackage(string projectFilePath, string packageName, NuGetVersion version, bool noRestore)
    {
        var status = new RunStatus(string.Empty, string.Empty, 0);

        try
        {
            var projectFile = _fileSystem.FileInfo.FromFileName(projectFilePath);
            var foundCPVMFile = false;
            var directoryInfo = projectFile.Directory;

            while (!foundCPVMFile && directoryInfo != null)
            {
                IFileInfo[] files = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly);
                var cpvmFile = files.SingleOrDefault(f => f.Name.Equals("Directory.Packages.Props", StringComparison.OrdinalIgnoreCase));

                if (cpvmFile != null)
                {
                    string fileContent;

                    using (var reader = cpvmFile.OpenText())
                    {
                        fileContent = reader.ReadToEnd();
                    }

                    if (fileContent.IndexOf($"\"{packageName}\"", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        var newFileContent = Regex.Replace(fileContent, $"(<PackageVersion\\s*(?:Include|Update)=\"{packageName}\"\\s*Version=\")([^\"]*)(\".*\\/>)", m => $"{m.Groups[1].Captures[0].Value}{version}{m.Groups[3].Captures[0].Value}");

                        if (newFileContent != fileContent)
                        {
                            _fileSystem.File.WriteAllText(cpvmFile.FullName, newFileContent);
                        }

                        foundCPVMFile = true;
                    }
                }

                if (!foundCPVMFile)
                {
                    directoryInfo = directoryInfo.Parent;
                }
            }

            if (!noRestore)
            {
                var restoreStatus = _dotNetRestoreService.Restore(projectFilePath);

                if (!restoreStatus.IsSuccess)
                {
                    status = new RunStatus(string.Empty, "Failed to restore project after upgrading!", -1);
                }
            }
        }
        catch (Exception)
        {
            status = new RunStatus(string.Empty, "Failed to update the central package version management file!", -1);
        }

        return status;
    }
}
