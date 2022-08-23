using DotNetOutdated.Models;
using NuGet.Versioning;
using Xunit;

namespace DotNetOutdated.Tests;

public class VersionNumberColoringTests
{
    [Theory]
    [InlineData("1.2.3    ", "2.0.0    ")]
    [InlineData("1.0.13   ", "2.0.1    ")]
    [InlineData("12.15.16 ", "13.0.0   ")]
    public void ColorsEntireVersionNumberForMajorUpgrades(string resolved, string latest)
    {
        var resolvedVersion = new NuGetVersion(resolved);
        var latestVersion = new NuGetVersion(latest);

        var console = new MockConsole();

        DotnetOutdatedCommandHandler.WriteColoredUpgrade(DependencyUpgradeSeverity.Major, resolvedVersion, latestVersion, 9, 9, console);

        Assert.Equal($"{resolved} -> [Red]{latest}[White]", console.WrittenOut);
    }

    [Theory]
    [InlineData("1.0.1-al ", "1.0.1-be ")]
    [InlineData("1.0.2-al ", "1.0.2    ")]
    public void ColorsEntireVersionNumberForPrereleaseUpgrades(string resolved, string latest)
    {
        var resolvedVersion = new NuGetVersion(resolved);
        var latestVersion = new NuGetVersion(latest);

        var console = new MockConsole();

        DotnetOutdatedCommandHandler.WriteColoredUpgrade(DependencyUpgradeSeverity.Major, resolvedVersion, latestVersion, 9, 9, console);

        Assert.Equal($"{resolved} -> [Red]{latest}[White]", console.WrittenOut);
    }

    [Theory]
    [InlineData("1.2.3    ", "1.3.0    ")]
    [InlineData("1.0.13   ", "1.4.13   ")]
    [InlineData("12.0.16  ", "12.18.0  ")]
    public void ColorsMinorAndPatchForMinorUpgrades(string resolved, string latest)
    {
        var resolvedVersion = new NuGetVersion(resolved);
        var latestVersion = new NuGetVersion(latest);

        var console = new MockConsole();

        DotnetOutdatedCommandHandler.WriteColoredUpgrade(DependencyUpgradeSeverity.Minor, resolvedVersion, latestVersion, 9, 9, console);
        var firstDot = latest.IndexOf(".") + 1;
        Assert.Equal($"{resolved} -> {latest[..firstDot]}[Yellow]{latest[firstDot..]}[White]", console.WrittenOut);
    }


    [Theory]
    [InlineData("1.2.3    ", "1.2.4    ")]
    [InlineData("1.0.13   ", "1.0.20   ")]
    [InlineData("12.0.16  ", "12.0.1542")]
    public void ColorsPatchForPatchUpgrades(string resolved, string latest)
    {
        var resolvedVersion = new NuGetVersion(resolved);
        var latestVersion = new NuGetVersion(latest);

        var console = new MockConsole();

        DotnetOutdatedCommandHandler.WriteColoredUpgrade(DependencyUpgradeSeverity.Patch, resolvedVersion, latestVersion, 9, 9, console);
        var secondDot = latest.IndexOf(".", latest.IndexOf(".") + 1) + 1;
        Assert.Equal($"{resolved} -> {latest[..secondDot]}[Green]{latest[secondDot..]}[White]", console.WrittenOut);
    }
}
