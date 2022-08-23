using DotNetOutdated.Core.Models;

namespace DotNetOutdated.Core.Services;

public interface IProjectAnalysisService
{
    IEnumerable<Project> AnalyzeProject(string projectPath, bool runRestore, bool includeTransitiveDependencies, int transitiveDepth);
}
