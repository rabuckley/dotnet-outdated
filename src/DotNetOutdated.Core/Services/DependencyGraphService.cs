﻿using System.IO.Abstractions;
using DotNetOutdated.Core.Exceptions;
using NuGet.ProjectModel;

namespace DotNetOutdated.Core.Services;

/// <remarks>
/// Credit for the stuff happening in here goes to the https://github.com/jaredcnance/dotnet-status project
/// </remarks>
public class DependencyGraphService : IDependencyGraphService
{
    private readonly IDotNetRunner _dotNetRunner;
    private readonly IFileSystem _fileSystem;

    public DependencyGraphService(IDotNetRunner dotNetRunner, IFileSystem fileSystem)
    {
        _dotNetRunner = dotNetRunner;
        _fileSystem = fileSystem;
    }

    public async Task<DependencyGraphSpec> GenerateDependencyGraphAsync(string projectPath)
    {
        var dgOutput = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), _fileSystem.Path.GetTempFileName());

        string[] arguments = {"msbuild", $"\"{projectPath}\"", "/t:Restore,GenerateRestoreGraphFile", $"/p:RestoreGraphOutputPath=\"{dgOutput}\""};

        var runStatus = await _dotNetRunner.Run(_fileSystem.Path.GetDirectoryName(projectPath), arguments).ConfigureAwait(false);

        if (!runStatus.IsSuccess)
        {
            throw new CommandValidationException(
                $"Unable to process the project `{projectPath}. Are you sure this is a valid .NET Core or .NET Standard project type?" +
                $"{Environment.NewLine}{Environment.NewLine}Here is the full error message returned from the Microsoft Build Engine:{Environment.NewLine}{Environment.NewLine}{runStatus.Output} - {runStatus.Errors} - exit code: {runStatus.ExitCode}");
        }

        using var tempDirectory = new TempDirectory();
        var dependencyGraphFilename = Path.Combine(tempDirectory.DirectoryPath, "DependencyGraph.json");
        var dependencyGraphText = await _fileSystem.File.ReadAllTextAsync(dgOutput).ConfigureAwait(false);
        await File.WriteAllTextAsync(dependencyGraphFilename, dependencyGraphText).ConfigureAwait(false);
        return DependencyGraphSpec.Load(dependencyGraphFilename);
    }
}
