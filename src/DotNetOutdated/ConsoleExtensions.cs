using McMaster.Extensions.CommandLineUtils;

namespace DotNetOutdated;

public static class ConsoleExtensions
{
    public static void Write(this IConsole console, object value, ConsoleColor color)
    {
        var currentColor = console.ForegroundColor;

        console.ForegroundColor = color;
        console.Write(value);
        console.ForegroundColor = currentColor;
    }

    public static void Write(this IConsole console, object value, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
    {
        var currentForegroundColor = console.ForegroundColor;
        var currentBackgroundColor = console.BackgroundColor;

        console.ForegroundColor = foregroundColor;
        console.BackgroundColor = backgroundColor;
        console.Write(value);
        console.ForegroundColor = currentForegroundColor;
        console.BackgroundColor = currentBackgroundColor;
    }

    public static void WriteLine(this IConsole console, object value, ConsoleColor color)
    {
        var currentColor = console.ForegroundColor;

        console.ForegroundColor = color;
        console.WriteLine(value);
        console.ForegroundColor = currentColor;
    }

    public static void WriteIndent(this IConsole console)
    {
        console.Write(new string(' ', 2));
    }
}
