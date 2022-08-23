using System.IO.Abstractions;
using DotNetOutdated.Core.Exceptions;
using DotNetOutdated.Core.Resources;

namespace DotNetOutdated.Core.Services;

public class ProjectDiscoveryService : IProjectDiscoveryService
{
    private readonly IFileSystem _fileSystem;

    public ProjectDiscoveryService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public IList<string> DiscoverProjects(string path, bool recursive = false)
    {
        if (!(_fileSystem.File.Exists(path) || _fileSystem.Directory.Exists(path)))
        {
            throw new CommandValidationException(string.Format(ValidationErrorMessages.DirectoryOrFileDoesNotExist,
                path));
        }

        var fileAttributes = _fileSystem.File.GetAttributes(path);

        // If a directory was passed in, search for a .sln or .csproj file
        if (fileAttributes.HasFlag(FileAttributes.Directory))
        {
            // If we are in recursive mode, find all individual projects recursively
            if (recursive)
            {
                var recursiveProjectFiles = _fileSystem.Directory
                    .GetFiles(path, "*.csproj", SearchOption.AllDirectories)
                    .Concat(_fileSystem.Directory.GetFiles(path, "*.fsproj", SearchOption.AllDirectories)).ToArray();

                if (recursiveProjectFiles.Length > 0)
                {
                    return recursiveProjectFiles;
                }

                // At this point the path contains no solutions or projects, so throw an exception
                throw new CommandValidationException(
                    string.Format(ValidationErrorMessages.DirectoryDoesNotContainSolutionsOrProjects, path));
            }

            // Search for solution(s)
            var solutionFiles = _fileSystem.Directory.GetFiles(path, "*.sln");
            switch (solutionFiles.Length)
            {
                case 1:
                    return new[] { _fileSystem.Path.GetFullPath(solutionFiles[0]) };
                case > 1:
                    throw new CommandValidationException(
                        string.Format(ValidationErrorMessages.DirectoryContainsMultipleSolutions, path));
            }

            // We did not find any solutions, so try and find individual projects
            var projectFiles = _fileSystem.Directory.GetFiles(path, "*.csproj")
                .Concat(_fileSystem.Directory.GetFiles(path, "*.fsproj")).ToArray();
            return projectFiles.Length switch
            {
                1 => new[] { _fileSystem.Path.GetFullPath(projectFiles[0]) },
                > 1 => throw new CommandValidationException(string.Format(ValidationErrorMessages.DirectoryContainsMultipleProjects, path)),
                _ => throw new CommandValidationException(string.Format(ValidationErrorMessages.DirectoryDoesNotContainSolutionsOrProjects, path))
            };

            // At this point the path contains no solutions or projects, so throw an exception
        }

        // If a .sln or .csproj file was passed, just return that
        if (string.Equals(_fileSystem.Path.GetExtension(path), ".sln", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(_fileSystem.Path.GetExtension(path), ".csproj", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(_fileSystem.Path.GetExtension(path), ".fsproj", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(_fileSystem.Path.GetExtension(path), ".slnf", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { _fileSystem.Path.GetFullPath(path) };
        }

        // At this point, we know the file passed in is not a valid project or solution
        throw new CommandValidationException(
            string.Format(ValidationErrorMessages.FileNotAValidSolutionOrProject, path));
    }
}
