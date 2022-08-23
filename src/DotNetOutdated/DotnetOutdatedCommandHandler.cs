// Copyright (c) Down Syndrome Education Enterprises CIC. All Rights Reserved.
// Information contained herein is PROPRIETARY AND CONFIDENTIAL.

using System.Collections.Concurrent;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using DotNetOutdated.Core.Exceptions;
using DotNetOutdated.Core.Models;
using DotNetOutdated.Core.Services;
using DotNetOutdated.Models;
using DotNetOutdated.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Common;
using NuGet.Credentials;
using NuGet.Versioning;

namespace DotNetOutdated;

public class DotnetOutdatedCommandHandler
{
    private CommandModel Model { get; }

    private readonly IConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IProjectDiscoveryService _projectDiscoveryService;
    private readonly IProjectAnalysisService _projectAnalysisService;
    private readonly IReporter _reporter;
    private readonly ICentralPackageVersionManagementService _centralPackageVersionManagementService;
    private readonly IDotNetAddPackageService _dotNetAddPackageService;
    private readonly INuGetPackageResolutionService _nugetPackageResolutionService;

    public DotnetOutdatedCommandHandler(InvocationContext context, CommandModel model)
    {
        Model = model;
        _console = context.BindingContext.GetService<IConsole>() ?? throw new InvalidOperationException($"{nameof(IConsole)} service not found");
        _fileSystem = context.BindingContext.GetService<IFileSystem>() ?? throw new InvalidOperationException($"{nameof(IFileSystem)} service not found");
        _projectDiscoveryService = context.BindingContext.GetService<IProjectDiscoveryService>() ?? throw new InvalidOperationException($"{nameof(IProjectDiscoveryService)} service not found");
        _projectAnalysisService = context.BindingContext.GetService<IProjectAnalysisService>() ?? throw new InvalidOperationException($"{nameof(IProjectAnalysisService)} service not found");
        _reporter = context.BindingContext.GetService<IReporter>() ?? throw new InvalidOperationException($"{nameof(IReporter)} service not found");
        _centralPackageVersionManagementService = context.BindingContext.GetService<ICentralPackageVersionManagementService>() ?? throw new InvalidOperationException($"{nameof(ICentralPackageVersionManagementService)} service not found");
        _dotNetAddPackageService = context.BindingContext.GetService<IDotNetAddPackageService>() ?? throw new InvalidOperationException($"{nameof(IDotNetAddPackageService)} service not found");
        _nugetPackageResolutionService = context.BindingContext.GetService<INuGetPackageResolutionService>() ?? throw new InvalidOperationException($"{nameof(INuGetPackageResolutionService)} service not found");
    }

    public async Task<int> ExecuteAsync()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // If no path is set, use the current directory
            if (string.IsNullOrEmpty(Model.Path))
            {
                Model.Path = _fileSystem.Directory.GetCurrentDirectory();
            }

            // Get all the projects
            _console.Write("Discovering projects...");

            DefaultCredentialServiceUtility.SetupDefaultCredentialService(new NullLogger(), true);

            var projectPaths = _projectDiscoveryService.DiscoverProjects(Model.Path, Model.Recursive);

            if (!_console.IsOutputRedirected)
            {
                ClearCurrentConsoleLine();
            }
            else
            {
                _console.WriteLine();
            }

            // Analyze the projects
            _console.Write("Analyzing project(s)...");

            var projects = projectPaths.SelectMany(path =>
                _projectAnalysisService.AnalyzeProject(path, false, Model.Transitive, Model.TransitiveDepth)).ToList();

            if (!_console.IsOutputRedirected)
            {
                ClearCurrentConsoleLine();
            }
            else
            {
                _console.WriteLine();
            }

            // Analyze the dependencies
            var outdatedProjects = await AnalyzeDependencies(projects).ConfigureAwait(false);

            if (outdatedProjects.Any())
            {
                ReportOutdatedDependencies(outdatedProjects);

                var success = UpgradePackages(outdatedProjects);

                // Output report file
                if (Model.OutputFilename is not null)
                {
                    GenerateOutputFile(outdatedProjects);
                }

                if (Model.FailOnUpdates)
                {
                    return 2;
                }

                if (!success)
                {
                    return 3;
                }
            }
            else
            {
                _console.WriteLine("No outdated dependencies were detected");
            }

            _console.WriteLine($"Elapsed: {stopwatch.Elapsed}");

            return 0;
        }
        catch (CommandValidationException e)
        {
            _reporter.Error(e.Message);

            return 1;
        }
    }

    private bool UpgradePackages(List<AnalyzedProject> projects)
    {
        var success = true;
        _console.WriteLine();

        var consolidatedPackages = projects.ConsolidatePackages();

        foreach (var package in consolidatedPackages)
        {
            var upgradePackage = true;

            if (Model.Upgrade == UpgradeType.Prompt)
            {
                var resolvedVersion = package.ResolvedVersion.ToString();
                var latestVersion = package.LatestVersion.ToString();

                _console.Write("The package ");
                _console.Write(package.Description, ReportingColors.PackageName);
                _console.Write($" can be upgraded from {resolvedVersion} to ");
                _console.Write(latestVersion, GetUpgradeSeverityColor(package.UpgradeSeverity));
                _console.WriteLine(". The following project(s) will be affected:");
                foreach (var project in package.Projects)
                {
                    WriteProjectName(project.Description);
                }

                upgradePackage = Prompt.GetYesNo("Do you wish to upgrade this package?", true);
            }

            if (upgradePackage)
            {
                _console.Write("Upgrading package ");
                _console.Write(package.Description, ReportingColors.PackageName);
                _console.Write("...");
                _console.WriteLine();

                foreach (var project in package.Projects)
                {
                    var status = package.IsVersionCentrallyManaged
                        ? _centralPackageVersionManagementService.AddPackage(project.ProjectFilePath, package.Name, package.LatestVersion, Model.NoRestore)
                        : _dotNetAddPackageService.AddPackage(project.ProjectFilePath, package.Name, project.Framework.ToString(), package.LatestVersion, Model.NoRestore, Model.IgnoreFailedSources);

                    if (status.IsSuccess)
                    {
                        _console.Write($"Project {project.Description} upgraded successfully", ReportingColors.UpgradeSuccess);
                        _console.WriteLine();
                    }
                    else
                    {
                        success = false;
                        _console.Write($"An error occurred while upgrading {project.Project}", ReportingColors.UpgradeFailure);
                        _console.WriteLine();
                        _console.Write(status.Errors, ReportingColors.UpgradeFailure);
                        _console.WriteLine();
                    }
                }
            }

            _console.WriteLine();
        }

        return success;
    }

    private void PrintColorLegend()
    {
        _console.WriteLine("Version color legend:");

        _console.Write("<red>".PadRight(8), ReportingColors.MajorVersionUpgrade);
        _console.WriteLine(": Major version update or pre-release version. Possible breaking changes.");
        _console.Write("<yellow>".PadRight(8), ReportingColors.MinorVersionUpgrade);
        _console.WriteLine(": Minor version update. Backwards-compatible features added.");
        _console.Write("<green>".PadRight(8), ReportingColors.PatchVersionUpgrade);
        _console.WriteLine(": Patch version update. Backwards-compatible bug fixes.");
    }

    internal static void WriteColoredUpgrade(DependencyUpgradeSeverity? upgradeSeverity, NuGetVersion? resolvedVersion, NuGetVersion? latestVersion, int resolvedWidth, int latestWidth, IConsole console)
    {
        console.Write((resolvedVersion?.ToString() ?? "").PadRight(resolvedWidth));
        console.Write(" -> ");

        // Exit early to avoid having to handle nulls later
        if (latestVersion is null)
        {
            console.Write("".PadRight(resolvedWidth));
            return;
        }

        var latestString = latestVersion.ToString().PadRight(latestWidth);
        if (resolvedVersion is null)
        {
            console.Write(latestString);
            return;
        }

        if (resolvedVersion.IsPrerelease)
        {
            console.Write(latestString, GetUpgradeSeverityColor(upgradeSeverity));
            return;
        }

        var matching = string.Join(".", resolvedVersion.GetParts()
            .Zip(latestVersion.GetParts(), (p1, p2) => (part: p2, matches: p1 == p2))
            .TakeWhile(p => p.matches)
            .Select(p => p.part));

        if (matching.Length > 0)
        {
            matching += ".";
        }

        var rest = new Regex($"^{matching}").Replace(latestString, "");

        console.Write($"{matching}");
        console.Write(rest, GetUpgradeSeverityColor(upgradeSeverity));
    }

    private void ReportOutdatedDependencies(List<AnalyzedProject> projects)
    {
        foreach (var project in projects)
        {
            WriteProjectName(project.Name);

            // Process each target framework with its related dependencies
            foreach (var targetFramework in project.TargetFrameworks)
            {
                WriteTargetFramework(targetFramework);

                var dependencies = targetFramework.Dependencies
                    .OrderBy(d => d.Name)
                    .ToList();

                var columnWidths = dependencies.DetermineColumnWidths();

                foreach (var dependency in dependencies)
                {
                    _console.WriteIndent();
                    _console.Write(dependency.Description.PadRight(columnWidths[0] + 2));

                    WriteColoredUpgrade(dependency.UpgradeSeverity, dependency.ResolvedVersion,
                        dependency.LatestVersion, columnWidths[1], columnWidths[2], _console);

                    _console.WriteLine();
                }
            }

            _console.WriteLine();
        }

        if (projects.SelectMany(p => p.TargetFrameworks).SelectMany(f => f.Dependencies)
            .Any(d => d.UpgradeSeverity == DependencyUpgradeSeverity.Unknown))
        {
            _console.WriteLine(
                "Errors occurred while analyzing dependencies for some of your projects. Are you sure you can connect to all your configured NuGet servers?",
                ConsoleColor.Red);
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")))
            {
                // Issue #255: Sometimes the dotnet executable sets this
                // variable for you, sometimes it doesn't. If it's not
                // present, credential providers will be skipped.
                _console.WriteLine();
                _console.WriteLine(
                    "Unable to find DOTNET_HOST_PATH environment variable. If you use credential providers for your NuGet sources you need to have this set to the path to the `dotnet` executable.",
                    ConsoleColor.Red);
            }

            _console.WriteLine();
        }

        PrintColorLegend();
    }

    private async Task<List<AnalyzedProject>> AnalyzeDependencies(IReadOnlyList<Project> projects)
    {
        var outdatedProjects = new ConcurrentBag<AnalyzedProject>();

        _console.Write("Analyzing dependencies...");

        var tasks = new Task[projects.Count];

        for (var index = 0; index < projects.Count; index++)
        {
            var project = projects[index];
            tasks[index] = AddOutdatedProjectsIfNeeded(project, outdatedProjects);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        if (!_console.IsOutputRedirected)
        {
            ClearCurrentConsoleLine();
        }
        else
        {
            _console.WriteLine();
        }

        return outdatedProjects.ToList();
    }

    private bool AnyIncludeFilterMatches(Dependency dep)
    {
        return Model.FilterInclude.Any(f => NameContains(dep, f));
    }

    private bool NoExcludeFilterMatches(Dependency dep)
    {
        return !Model.FilterExclude.Any(f => NameContains(dep, f));
    }

    private static bool NameContains(Dependency dep, string part)
    {
        return dep.Name.Contains(part, StringComparison.InvariantCultureIgnoreCase);
    }

    private async Task AddOutdatedProjectsIfNeeded(Project project, ConcurrentBag<AnalyzedProject> outdatedProjects)
    {
        var outdatedFrameworks = new ConcurrentBag<AnalyzedTargetFramework>();

        var tasks = new Task[project.TargetFrameworks.Count];

        // Process each target framework with its related dependencies
        for (var index = 0; index < project.TargetFrameworks.Count; index++)
        {
            var targetFramework = project.TargetFrameworks[index];
            tasks[index] = AddOutdatedFrameworkIfNeeded(targetFramework, project, outdatedFrameworks);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

#pragma warning disable CA1836
        if (outdatedFrameworks.Count > 0)
#pragma warning restore CA1836
        {
            outdatedProjects.Add(new AnalyzedProject(project.Name, project.FilePath, outdatedFrameworks));
        }
    }

    private async Task AddOutdatedFrameworkIfNeeded(TargetFramework targetFramework, Project project, ConcurrentBag<AnalyzedTargetFramework> outdatedFrameworks)
    {
        var outdatedDependencies = new ConcurrentBag<AnalyzedDependency>();

        var deps = targetFramework.Dependencies.Where(d => Model.IncludeAutoReferences || !d.IsAutoReferenced);

        if (Model.FilterInclude.Any())
        {
            deps = deps.Where(AnyIncludeFilterMatches);
        }

        if (Model.FilterExclude.Any())
        {
            deps = deps.Where(NoExcludeFilterMatches);
        }

        var dependencies = deps.OrderBy(dependency => dependency.IsTransitive)
            .ThenBy(dependency => dependency.Name)
            .ToList();

        var tasks = new Task[dependencies.Count];

        for (var index = 0; index < dependencies.Count; index++)
        {
            var dependency = dependencies[index];

            tasks[index] = AddOutdatedDependencyIfNeeded(project, targetFramework, dependency, outdatedDependencies);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

#pragma warning disable CA1836
        if (outdatedDependencies.Count > 0)
#pragma warning restore CA1836
        {
            outdatedFrameworks.Add(new AnalyzedTargetFramework(targetFramework.Name, outdatedDependencies));
        }
    }

    private async Task AddOutdatedDependencyIfNeeded(Project project, TargetFramework targetFramework, Dependency dependency, ConcurrentBag<AnalyzedDependency> outdatedDependencies)
    {
        var resolvedVersion = dependency.ResolvedVersion;
        NuGetVersion? latestVersion = null;

        if (resolvedVersion is not null)
        {
            latestVersion = await _nugetPackageResolutionService.ResolvePackageVersions(dependency.Name, resolvedVersion, project.Sources, dependency.VersionRange, targetFramework.Name, project.FilePath, dependency.IsDevelopmentDependency, Model).ConfigureAwait(false);
        }

        if (resolvedVersion is null || latestVersion is null || resolvedVersion != latestVersion)
        {
            // special case when there is version installed which is not older than "OlderThan" days makes "latestVersion" to be null
            if (Model.OlderThanDays > 0 && latestVersion is null)
            {
                var absoluteLatestVersion = await _nugetPackageResolutionService.ResolvePackageVersions(dependency.Name, resolvedVersion, project.Sources, dependency.VersionRange, Model, targetFramework.Name, project.FilePath, dependency.IsDevelopmentDependency).ConfigureAwait(false);

                if (resolvedVersion > absoluteLatestVersion)
                {
                    outdatedDependencies.Add(new AnalyzedDependency(dependency, latestVersion));
                }
            }
            else
            {
                outdatedDependencies.Add(new AnalyzedDependency(dependency, latestVersion));
            }
        }
    }

    private static ConsoleColor GetUpgradeSeverityColor(DependencyUpgradeSeverity? upgradeSeverity)
    {
        return upgradeSeverity switch
        {
            DependencyUpgradeSeverity.Major => ReportingColors.MajorVersionUpgrade,
            DependencyUpgradeSeverity.Minor => ReportingColors.MinorVersionUpgrade,
            DependencyUpgradeSeverity.Patch => ReportingColors.PatchVersionUpgrade,
            _ => Console.ForegroundColor
        };
    }

    private void GenerateOutputFile(List<AnalyzedProject> projects)
    {
        Console.WriteLine();
        Console.WriteLine($@"Generating {Model.OutputFileFormat} report...");

        var reportContent = Model.OutputFileFormat switch
        {
            OutputFormat.Csv => Report.GetCsvReportContent(projects),
            _ => Report.GetJsonReportContent(projects)
        };

        _fileSystem.File.WriteAllText(Model.OutputFilename, reportContent);

        Console.WriteLine($@"Report written to {Model.OutputFilename}");
        Console.WriteLine();
    }

    private void WriteProjectName(string name)
    {
        _console.Write($"» {name}", ReportingColors.ProjectName);
        _console.WriteLine();
    }

    private void WriteTargetFramework(AnalyzedTargetFramework targetFramework)
    {
        _console.WriteIndent();
        _console.Write($"[{targetFramework.Name}]", ReportingColors.TargetFrameworkName);
        _console.WriteLine();
    }

    private static void ClearCurrentConsoleLine()
    {
        var currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.BufferWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }
}
