using System.Globalization;
using CsvHelper;
using DotNetOutdated.Core.Models;
using DotNetOutdated.Models;
using Newtonsoft.Json;

namespace DotNetOutdated.Services;
internal class ToStringJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }

    public override bool CanRead =>
        false;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public class Report
{
    public List<AnalyzedProject> Projects { get; set; }

    internal static string GetTextReportLine(AnalyzedProject project, AnalyzedTargetFramework targetFramework, AnalyzedDependency dependency)
    {
        var upgradeSeverity = Enum.GetName(typeof(DependencyUpgradeSeverity), dependency.UpgradeSeverity);
        return $"{project.Name};{targetFramework.Name};{dependency.Name};{dependency.ResolvedVersion};{dependency.LatestVersion};{upgradeSeverity}";
    }

    public static string GetCsvReportContent(List<AnalyzedProject> projects)
    {
        using var sw = new StringWriter();
        using var csv = new CsvWriter(sw, CultureInfo.CurrentCulture);
        foreach (var project in projects)
        {
            foreach (var targetFramework in project.TargetFrameworks)
            {
                foreach (var dependency in targetFramework.Dependencies)
                {
                    var upgradeSeverity = Enum.GetName(typeof(DependencyUpgradeSeverity), dependency.UpgradeSeverity);

                    csv.WriteRecord(new
                    {
                        ProjectName = project.Name,
                        TargetFrameworkName = targetFramework.Name.DotNetFrameworkName,
                        DependencyName = dependency.Name,
                        ResolvedVersion = dependency.ResolvedVersion?.ToString(),
                        LatestVersion = dependency.LatestVersion?.ToString(),
                        UpgradeSeverity = upgradeSeverity
                    });
                    csv.NextRecord();
                }
            }
        }

        return sw.ToString();
    }

    public static string GetJsonReportContent(List<AnalyzedProject> projects)
    {
        var report = new Report
        {
            Projects = projects
        };
        return JsonConvert.SerializeObject(report, Formatting.Indented);
    }
}
