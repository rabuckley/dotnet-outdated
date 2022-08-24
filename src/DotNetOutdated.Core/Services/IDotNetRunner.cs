namespace DotNetOutdated.Core.Services;

public interface IDotNetRunner
{
    Task<RunStatus> Run(string workingDirectory, string[] arguments);
}
