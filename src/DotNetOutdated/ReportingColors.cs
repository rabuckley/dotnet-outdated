namespace DotNetOutdated;

public static class ReportingColors
{
    public const ConsoleColor ProjectName = ConsoleColor.Blue;
    public const ConsoleColor TargetFrameworkName = ConsoleColor.Cyan;
    public const ConsoleColor PackageName = ConsoleColor.Magenta;

    public const ConsoleColor MajorVersionUpgrade = ConsoleColor.Red;
    public const ConsoleColor MinorVersionUpgrade = ConsoleColor.Yellow;
    public const ConsoleColor PatchVersionUpgrade = ConsoleColor.Green;

    public const ConsoleColor UpgradeSuccess = ConsoleColor.Green;
    public const ConsoleColor UpgradeFailure = ConsoleColor.Red;
}
