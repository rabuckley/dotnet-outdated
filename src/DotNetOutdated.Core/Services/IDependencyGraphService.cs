using NuGet.ProjectModel;

namespace DotNetOutdated.Core.Services;

public interface IDependencyGraphService
{
    Task<DependencyGraphSpec> GenerateDependencyGraphAsync(string projectPath);
}
