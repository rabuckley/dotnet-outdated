﻿using System.IO.Abstractions;
using NuGet.Versioning;

namespace DotNetOutdated.Core.Services;

public class DotNetAddPackageService : IDotNetAddPackageService
{
    private readonly IDotNetRunner _dotNetRunner;
    private readonly IFileSystem _fileSystem;

    public DotNetAddPackageService(IDotNetRunner dotNetRunner, IFileSystem fileSystem)
    {
        _dotNetRunner = dotNetRunner;
        _fileSystem = fileSystem;
    }

    public async Task<RunStatus> AddPackage(string projectPath, string packageName, string frameworkName, NuGetVersion version, bool ignoreFailedSources)
    {
        var projectName = _fileSystem.Path.GetFileName(projectPath);

        var arguments = new List<string>
        {
            "add",
            $"\"{projectName}\"",
            "package",
            packageName,
            "-v",
            version.ToString(),
            "-f",
            $"\"{frameworkName}\"",
            "--no-restore"
        };

        if (ignoreFailedSources)
        {
            arguments.Add("--ignore-failed-sources");
        }

        return await _dotNetRunner.Run(_fileSystem.Path.GetDirectoryName(projectPath), arguments.ToArray()).ConfigureAwait(false);
    }
}
