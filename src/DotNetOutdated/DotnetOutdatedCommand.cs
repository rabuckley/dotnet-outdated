// Copyright (c) Down Syndrome Education Enterprises CIC. All Rights Reserved.
// Information contained herein is PROPRIETARY AND CONFIDENTIAL.

using System.CommandLine;
using DotNetOutdated.Core;
using DotNetOutdated.Core.Models;

namespace DotNetOutdated;

public sealed class DotnetOutdatedCommand : RootCommand
{
    public DotnetOutdatedCommand()
    {
        Name = "dotnet-outdated";
        Description = "A .NET Core global tool to list outdated Nuget packages.";

        var path = new Argument<string>("path", "The path to a .NET Core solution/project, or to a directory containing one. If none is specified, the current directory will be used.");
        AddArgument(path);

        var includeAutoReferences = new Option<bool>("--include-auto-references", "Specifies whether to include auto-referenced packages.");
        AddOption(includeAutoReferences);

        var preRelease = new Option<PrereleaseReporting>("--pre-release", "Specifies whether to look for pre-release versions of packages. ");
        preRelease.AddAlias("-pre");
        preRelease.SetDefaultValue(PrereleaseReporting.Auto);
        AddOption(preRelease);

        var versionLock = new Option<VersionLock>("--version-lock", "Specifies whether the package should be locked to the current Major or Minor version.");
        versionLock.AddAlias("-vl");
        versionLock.SetDefaultValue(VersionLock.Minor);
        AddOption(versionLock);

        var transitive = new Option<bool>("--transitive", "Specifies whether it should detect transitive dependencies.");
        transitive.AddAlias("-t");
        AddOption(transitive);

        var transitiveDepth = new Option<int>("--transitive-depth", "Specifies how many levels deep transitive dependencies should be analyzed.");
        transitiveDepth.AddAlias("-td");
        transitiveDepth.SetDefaultValue(1);
        AddOption(transitiveDepth);

        var upgrade = new Option<UpgradeType>("--upgrade", "Specifies whether outdated packages should be upgraded.");
        upgrade.AddAlias("-u");
        upgrade.SetDefaultValue(UpgradeType.Auto);
        AddOption(upgrade);

        var failOnUpdates = new Option<bool>("--fail-on-updates", "Specifies whether it should return a non-zero exit code when updates are found.");
        failOnUpdates.AddAlias("-f");
        AddOption(failOnUpdates);

        var filterInclude = new Option<List<string>>("--include", "Specifies to only look at packages where the name contains the provided string. Culture and case insensitive. If provided multiple times, a single match is enough to include a package.");
        filterInclude.AddAlias("-inc");
        AddOption(filterInclude);

        var filterExclude = new Option<List<string>>("--exclude", "Specifies to only look at packages where the name does not contain the provided string. Culture and case insensitive. If provided multiple times, a single match is enough to exclude a package.");
        filterExclude.AddAlias("-exc");
        AddOption(filterExclude);

        var outputFilename = new Option<string>("--output", "Specifies the filename for a generated report. (Use the -of|--output-format option to specify the format. JSON by default.)");
        outputFilename.AddAlias("-o");
        AddOption(outputFilename);

        var outputFileFormat = new Option<OutputFormat>("--output-format", "Specifies the output format for the generated report.");
        outputFileFormat.AddAlias("-of");
        outputFileFormat.SetDefaultValue(OutputFormat.Json);
        AddOption(outputFileFormat);

        var olderThanDays = new Option<int>("--older-than", "Specifies the number of days that a package version should be older than to be considered outdated.");
        olderThanDays.AddAlias("-ot");
        AddOption(olderThanDays);

        var noRestore = new Option<bool>("--no-restore", "Add the reference without performing restore preview and compatibility check.");
        noRestore.AddAlias("-n");
        AddOption(noRestore);

        var recursive = new Option<bool>("--recursive", "Recursively search for all projects within the provided directory.");
        recursive.AddAlias("-r");
        AddOption(recursive);

        var ignoreFailedSources = new Option<bool>("--ignore-failed-sources", "Treat package source failures as warnings.");
        ignoreFailedSources.AddAlias("-ifs");
        AddOption(ignoreFailedSources);

        this.SetHandler(context =>
        {
            var binder = new CommandBinder(path, includeAutoReferences, preRelease, versionLock, transitive, transitiveDepth, upgrade, failOnUpdates, filterInclude, filterExclude, outputFilename, outputFileFormat, olderThanDays, noRestore, recursive, ignoreFailedSources);
            var model = binder.Bind(context.BindingContext);
            var handler = new DotnetOutdatedCommandHandler(context, model);
            return handler.ExecuteAsync();
        });
    }
}
