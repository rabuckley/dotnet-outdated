using DotNetOutdated.Core.Models;

namespace DotNetOutdated.Core.Services;

public interface IProjectAnalysisService
{
    Task<IEnumerable<Project>> AnalyzeProjectAsync(string projectPath, bool runRestore, bool includeTransitiveDependencies, int transitiveDepth);
}
