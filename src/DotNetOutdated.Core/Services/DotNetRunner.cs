using System.Diagnostics;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace DotNetOutdated.Core.Services;

/// <remarks>
/// Credit for the stuff happening in here goes to the https://github.com/jaredcnance/dotnet-status project
/// </remarks>
public class DotNetRunner : IDotNetRunner
{
    public async Task<RunStatus> Run(string workingDirectory, string[] arguments)
    {
        var psi = new ProcessStartInfo(DotNetExe.FullPathOrDefault(), string.Join(" ", arguments))
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = new Process();
        try
        {
            process.StartInfo = psi;
            process.Start();

            var output = new StringBuilder();
            var errors = new StringBuilder();

            var outputTask = ConsumeStreamReaderAsync(process.StandardOutput, output);
            var errorTask = ConsumeStreamReaderAsync(process.StandardError, errors);


            await process.WaitForExitAsync().ConfigureAwait(false);

            await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);

            return new RunStatus(output.ToString(), errors.ToString(), process.ExitCode);
        }
        finally
        {
            process.Dispose();
        }
    }

    private static async Task ConsumeStreamReaderAsync(TextReader reader, StringBuilder lines)
    {
        await Task.Yield();

        while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            lines.AppendLine(line);
        }
    }
}
