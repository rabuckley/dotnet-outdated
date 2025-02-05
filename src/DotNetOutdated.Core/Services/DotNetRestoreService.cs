﻿using System.IO.Abstractions;

namespace DotNetOutdated.Core.Services;

public class DotNetRestoreService : IDotNetRestoreService
{
    private readonly IDotNetRunner _dotNetRunner;
    private readonly IFileSystem _fileSystem;

    public DotNetRestoreService(IDotNetRunner dotNetRunner, IFileSystem fileSystem)
    {
        _dotNetRunner = dotNetRunner;
        _fileSystem = fileSystem;
    }

    public async Task<RunStatus> RestoreAsync(string projectPath)
    {
        string[] arguments = {"restore", $"\"{projectPath}\""};
        return await _dotNetRunner.Run(_fileSystem.Path.GetDirectoryName(projectPath), arguments).ConfigureAwait(false);
    }
}
