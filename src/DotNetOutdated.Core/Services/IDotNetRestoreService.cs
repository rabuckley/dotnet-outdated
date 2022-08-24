namespace DotNetOutdated.Core.Services;

public interface IDotNetRestoreService
{
    Task<RunStatus> RestoreAsync(string projectPath);
}
