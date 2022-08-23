// Copyright (c) Down Syndrome Education Enterprises CIC. All Rights Reserved.
// Information contained herein is PROPRIETARY AND CONFIDENTIAL.

namespace DotNetOutdated.Core.Models;

public class CommandModel
{
    public string Path { get; set; } = default!;
    public bool IncludeAutoReferences { get; set; }
    public PrereleaseReporting PreRelease { get; set; }
    public bool Transitive { get; set; }
    public int TransitiveDepth { get; set; }
    public UpgradeType Upgrade { get; set; }
    public VersionLock VersionLock { get; set; }
    public bool FailOnUpdates { get; set; }
    public List<string> FilterInclude { get; set; } = new();
    public List<string> FilterExclude { get; set; } = new();
    public bool NoRestore { get; set; }
    public OutputFormat OutputFileFormat { get; set; }
    public string? OutputFilename { get; set; }
    public int OlderThanDays { get; set; }
    public bool IgnoreFailedSources { get; set; }
    public bool Recursive { get; set; }
}
