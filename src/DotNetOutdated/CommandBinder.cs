using System.CommandLine;
using System.CommandLine.Binding;
using DotNetOutdated.Core;
using DotNetOutdated.Core.Models;

namespace DotNetOutdated;

public class CommandBinder : BinderBase<CommandModel>
{
    private readonly Argument<string> _path;

    private readonly Option<bool> _includeAutoReferences;

    private readonly Option<PrereleaseReporting> _preRelease;

    private readonly Option<VersionLock> _versionLock;

    private readonly Option<bool> _transitive;

    private readonly Option<int> _transitiveDepth;

    private readonly Option<UpgradeType> _upgrade;

    private readonly Option<bool> _failOnUpdates;

    private readonly Option<List<string>> _filterInclude;

    private readonly Option<List<string>> _filterExclude;

    private readonly Option<string> _outputFilename;

    private readonly Option<OutputFormat> _outputFileFormat;

    private readonly Option<int> _olderThanDays;

    private readonly Option<bool> _noRestore;

    private readonly Option<bool> _recursive;

    private readonly Option<bool> _ignoreFailedSources;

    public CommandBinder(Argument<string> path, Option<bool> includeAutoReferences, Option<PrereleaseReporting> preRelease, Option<VersionLock> versionLock, Option<bool> transitive, Option<int> transitiveDepth, Option<UpgradeType> upgrade, Option<bool> failOnUpdates, Option<List<string>> filterInclude, Option<List<string>> filterExclude, Option<string> outputFilename, Option<OutputFormat> outputFileFormat, Option<int> olderThanDays, Option<bool> noRestore, Option<bool> recursive, Option<bool> ignoreFailedSources)
    {
        _path = path;
        _includeAutoReferences = includeAutoReferences;
        _preRelease = preRelease;
        _versionLock = versionLock;
        _transitive = transitive;
        _transitiveDepth = transitiveDepth;
        _upgrade = upgrade;
        _failOnUpdates = failOnUpdates;
        _filterInclude = filterInclude;
        _filterExclude = filterExclude;
        _outputFilename = outputFilename;
        _outputFileFormat = outputFileFormat;
        _olderThanDays = olderThanDays;
        _noRestore = noRestore;
        _recursive = recursive;
        _ignoreFailedSources = ignoreFailedSources;
    }

    public CommandModel Bind(BindingContext context)
    {
        return GetBoundValue(context);
    }

    protected override CommandModel GetBoundValue(BindingContext bindingContext)
    {
        var path = bindingContext.ParseResult.GetValueForArgument(_path);
        var includeAutoReferences = bindingContext.ParseResult.GetValueForOption(_includeAutoReferences);
        var preRelease = bindingContext.ParseResult.GetValueForOption(_preRelease);
        var transitive = bindingContext.ParseResult.GetValueForOption(_transitive);
        var transitiveDepth = bindingContext.ParseResult.GetValueForOption(_transitiveDepth);
        var upgrade = bindingContext.ParseResult.GetValueForOption(_upgrade);
        var versionLock = bindingContext.ParseResult.GetValueForOption(_versionLock);
        var failOnUpdates = bindingContext.ParseResult.GetValueForOption(_failOnUpdates);
        var olderThanDays = bindingContext.ParseResult.GetValueForOption(_olderThanDays);
        var filterInclude = bindingContext.ParseResult.GetValueForOption(_filterInclude);
        var filterExclude = bindingContext.ParseResult.GetValueForOption(_filterExclude);
        var noRestore = bindingContext.ParseResult.GetValueForOption(_noRestore);
        var outputFileFormat = bindingContext.ParseResult.GetValueForOption(_outputFileFormat);
        var outputFilename = bindingContext.ParseResult.GetValueForOption(_outputFilename);
        var ignoreFailedSources = bindingContext.ParseResult.GetValueForOption(_ignoreFailedSources);
        var recursive = bindingContext.ParseResult.GetValueForOption(_recursive);

        return new CommandModel
        {
            Path = path,
            IncludeAutoReferences = includeAutoReferences,
            PreRelease = preRelease,
            Transitive = transitive,
            TransitiveDepth = transitiveDepth,
            Upgrade = upgrade,
            VersionLock = versionLock,
            FailOnUpdates = failOnUpdates,
            OlderThanDays = olderThanDays,
            FilterInclude = filterInclude ?? new List<string>(),
            FilterExclude = filterExclude ?? new List<string>(),
            NoRestore = noRestore,
            OutputFileFormat = outputFileFormat,
            OutputFilename = outputFilename,
            IgnoreFailedSources = ignoreFailedSources,
            Recursive = recursive
        };
    }
}
